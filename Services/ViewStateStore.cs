using System.Text.Json;

namespace GWxLauncher.Services
{
    public sealed class ViewStateStore
    {
        private const string DefaultViewName = "Default";
        private const string FileName = "views.json";

        // viewName -> (profileId -> eligible)
        private readonly Dictionary<string, Dictionary<string, bool>> _eligibilityByView =
            new(StringComparer.CurrentCultureIgnoreCase);
        // viewName -> showCheckedOnly
        private readonly Dictionary<string, bool> _showCheckedOnlyByView =
            new(StringComparer.CurrentCultureIgnoreCase);


        public List<string> ViewNames { get; private set; } = new();
        public string ActiveViewName { get; private set; } = DefaultViewName;

        public void Load()
        {
            var path = GetViewsPath();

            if (!File.Exists(path))
            {
                EnsureViewExists(DefaultViewName);
                ActiveViewName = DefaultViewName;
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                var dto = JsonSerializer.Deserialize<ViewsDto>(json);

                if (dto == null)
                    throw new InvalidOperationException("views.json deserialized null");

                // Normalize view list
                ViewNames = (dto.ViewNames ?? new List<string>())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                if (ViewNames.Count == 0)
                    ViewNames.Add(DefaultViewName);

                // Eligibility map
                _eligibilityByView.Clear();
                if (dto.EligibilityByView != null)
                {
                    foreach (var kv in dto.EligibilityByView)
                    {
                        if (string.IsNullOrWhiteSpace(kv.Key))
                            continue;

                        _eligibilityByView[kv.Key] = kv.Value ?? new Dictionary<string, bool>();
                    }
                }
                // ShowCheckedOnly map (view-scoped)
                _showCheckedOnlyByView.Clear();
                if (dto.ShowCheckedOnlyByView != null)
                {
                    foreach (var kv in dto.ShowCheckedOnlyByView)
                    {
                        if (string.IsNullOrWhiteSpace(kv.Key))
                            continue;

                        _showCheckedOnlyByView[kv.Key] = kv.Value;
                    }
                }

                // Active view
                ActiveViewName = string.IsNullOrWhiteSpace(dto.ActiveViewName)
                    ? ViewNames[0]
                    : dto.ActiveViewName.Trim();

                EnsureViewExists(ActiveViewName);
            }
            catch
            {
                // If file is corrupted, fall back safely (no crash)
                ViewNames = new List<string> { DefaultViewName };
                _eligibilityByView.Clear();
                EnsureViewExists(DefaultViewName);
                ActiveViewName = DefaultViewName;
            }
        }

        public void Save()
        {
            EnsureViewExists(ActiveViewName);

            var dto = new ViewsDto
            {
                ActiveViewName = ActiveViewName,
                ViewNames = ViewNames,
                EligibilityByView = _eligibilityByView,
                ShowCheckedOnlyByView = _showCheckedOnlyByView
            };

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });

            var path = GetViewsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
        }

        public void SetActiveView(string viewName)
        {
            EnsureViewExists(viewName);
            ActiveViewName = viewName;
        }

        public string StepActiveView(int delta)
        {
            if (ViewNames.Count == 0)
                EnsureViewExists(DefaultViewName);

            var idx = ViewNames.FindIndex(v => v.Equals(ActiveViewName, StringComparison.CurrentCultureIgnoreCase));
            if (idx < 0) idx = 0;

            idx = (idx + delta) % ViewNames.Count;
            if (idx < 0) idx += ViewNames.Count;

            ActiveViewName = ViewNames[idx];
            EnsureViewExists(ActiveViewName);
            return ActiveViewName;
        }

        public string CreateNewView(string baseName)
        {
            var name = (baseName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                name = "New View";

            var candidate = name;
            var i = 1;
            while (ViewNames.Any(v => string.Equals(v, candidate, StringComparison.CurrentCultureIgnoreCase)))
                candidate = $"{name} {i++}";

            EnsureViewExists(candidate);
            ActiveViewName = candidate;
            return candidate;
        }

        public bool RenameActiveView(string newName)
        {
            newName = (newName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newName))
                return false;

            if (string.Equals(newName, ActiveViewName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            if (ViewNames.Any(v => string.Equals(v, newName, StringComparison.CurrentCultureIgnoreCase)))
                return false; // name already exists

            // rename in list
            var idx = ViewNames.FindIndex(v => v.Equals(ActiveViewName, StringComparison.CurrentCultureIgnoreCase));
            if (idx >= 0)
                ViewNames[idx] = newName;

            // move eligibility map
            if (_eligibilityByView.TryGetValue(ActiveViewName, out var map))
            {
                _eligibilityByView.Remove(ActiveViewName);
                _eligibilityByView[newName] = map;
            }
            else
            {
                _eligibilityByView[newName] = new Dictionary<string, bool>();
            }

            ActiveViewName = newName;
            return true;
        }
        public bool GetShowCheckedOnly(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                viewName = ActiveViewName;

            EnsureViewExists(viewName);

            if (_showCheckedOnlyByView.TryGetValue(viewName, out var v))
                return v;

            return false;
        }

        public void SetShowCheckedOnly(string viewName, bool value)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                viewName = ActiveViewName;

            EnsureViewExists(viewName);
            _showCheckedOnlyByView[viewName] = value;
        }

        public bool IsEligible(string viewName, string profileId)
        {
            if (string.IsNullOrWhiteSpace(viewName) || string.IsNullOrWhiteSpace(profileId))
                return false;

            EnsureViewExists(viewName);

            if (_eligibilityByView.TryGetValue(viewName, out var map) &&
                map.TryGetValue(profileId, out var eligible))
            {
                return eligible;
            }

            return false;
        }

        public void SetEligible(string viewName, string profileId, bool eligible)
        {
            if (string.IsNullOrWhiteSpace(viewName) || string.IsNullOrWhiteSpace(profileId))
                return;

            EnsureViewExists(viewName);

            var map = _eligibilityByView[viewName];
            map[profileId] = eligible;
        }

        public void ToggleEligible(string viewName, string profileId)
        {
            var current = IsEligible(viewName, profileId);
            SetEligible(viewName, profileId, !current);
        }

        private void EnsureViewExists(string viewName)
        {
            viewName = (viewName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(viewName))
                viewName = DefaultViewName;

            if (!ViewNames.Any(v => v.Equals(viewName, StringComparison.CurrentCultureIgnoreCase)))
                ViewNames.Add(viewName);

            if (!_eligibilityByView.ContainsKey(viewName))
                _eligibilityByView[viewName] = new Dictionary<string, bool>();

            if (!_showCheckedOnlyByView.ContainsKey(viewName))
                _showCheckedOnlyByView[viewName] = false;
        }

        private static string GetViewsPath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(root, "GWxLauncher", FileName);
        }
        public bool AnyEligibleInActiveView(IEnumerable<Domain.GameProfile> profiles)
        {
            return profiles.Any(p => IsEligible(ActiveViewName, p.Id));
        }

        private sealed class ViewsDto
        {
            public string? ActiveViewName { get; set; }
            public List<string>? ViewNames { get; set; }
            public Dictionary<string, Dictionary<string, bool>>? EligibilityByView { get; set; }
            public Dictionary<string, bool>? ShowCheckedOnlyByView { get; set; }
        }
    }
}
