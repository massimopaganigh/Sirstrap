namespace Sirstrap.UI.ViewModels
{
    public partial class FFlagEditorWindowViewModel : ViewModelBase
    {
        private readonly IFFlagManager _fflagManager;

        [ObservableProperty]
        private ObservableCollection<FFlagEntry> _flags = [];

        [ObservableProperty]
        private ObservableCollection<FFlagEntry> _filteredFlags = [];

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _rawJsonText = string.Empty;

        [ObservableProperty]
        private int _selectedTabIndex;

        [ObservableProperty]
        private string _newFlagName = string.Empty;

        [ObservableProperty]
        private string _newFlagValue = string.Empty;

        // Presets
        [ObservableProperty]
        private string _selectedFpsPreset = "Default";

        [ObservableProperty]
        private string _selectedGraphicsApiPreset = "Default";

        [ObservableProperty]
        private string _selectedMsaaPreset = "Default";

        public ObservableCollection<string> FpsPresetOptions { get; } = ["Default", "60", "120", "144", "240", "360", "Uncapped (9999)"];

        public ObservableCollection<string> GraphicsApiPresetOptions { get; } = ["Default", "Direct3D11", "Direct3D10", "Vulkan", "OpenGL"];

        public ObservableCollection<string> MsaaPresetOptions { get; } = ["Default", "Off (0)", "2x", "4x", "8x"];

        public FFlagEditorWindowViewModel(IFFlagManager fflagManager)
        {
            _fflagManager = fflagManager;

            LoadFlags();
        }

        public void LoadFlags()
        {
            try
            {
                var dict = _fflagManager.LoadFFlags();
                var entries = dict.Select(kv => new FFlagEntry
                {
                    Name = kv.Key,
                    Value = kv.Value?.ToString() ?? string.Empty
                }).OrderBy(x => x.Name);

                Flags = new ObservableCollection<FFlagEntry>(entries);
                UpdateFilteredFlags();
                UpdateRawJsonText();
                DetectPresets();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(LoadFlags));
            }
        }

        partial void OnSearchTextChanged(string value) => UpdateFilteredFlags();

        partial void OnSelectedTabIndexChanged(int value)
        {
            if (value == 1) // Switched to Raw JSON tab
            {
                UpdateRawJsonText();
            }
            else if (value == 0) // Switched back to Table view tab
            {
                ParseRawJsonToFlags();
            }
        }

        private void UpdateFilteredFlags()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredFlags = new ObservableCollection<FFlagEntry>(Flags);
            }
            else
            {
                var filtered = Flags.Where(f =>
                    f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    f.Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                FilteredFlags = new ObservableCollection<FFlagEntry>(filtered);
            }
        }

        private void UpdateRawJsonText()
        {
            try
            {
                var dict = FlagsToDictionary();
                var options = new JsonSerializerOptions { WriteIndented = true };
                RawJsonText = JsonSerializer.Serialize(dict, options);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(UpdateRawJsonText));
            }
        }

        private void ParseRawJsonToFlags()
        {
            if (string.IsNullOrWhiteSpace(RawJsonText))
                return;

            try
            {
                using var doc = JsonDocument.Parse(RawJsonText);
                var newFlags = new List<FFlagEntry>();

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    newFlags.Add(new FFlagEntry
                    {
                        Name = prop.Name,
                        Value = prop.Value.ToString()
                    });
                }

                Flags = new ObservableCollection<FFlagEntry>(newFlags.OrderBy(x => x.Name));
                UpdateFilteredFlags();
                DetectPresets();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, nameof(ParseRawJsonToFlags));
            }
        }

        private Dictionary<string, object> FlagsToDictionary()
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var flag in Flags)
            {
                if (string.IsNullOrWhiteSpace(flag.Name))
                    continue;

                var trimmedValue = flag.Value.Trim();

                if (bool.TryParse(trimmedValue, out var boolVal))
                    dict[flag.Name] = boolVal;
                else if (long.TryParse(trimmedValue, out var longVal))
                    dict[flag.Name] = longVal;
                else if (double.TryParse(trimmedValue, out var doubleVal))
                    dict[flag.Name] = doubleVal;
                else
                    dict[flag.Name] = trimmedValue;
            }

            return dict;
        }

        [RelayCommand]
        private void AddFlag()
        {
            if (string.IsNullOrWhiteSpace(NewFlagName))
                return;

            var existing = Flags.FirstOrDefault(f => f.Name.Equals(NewFlagName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Value = NewFlagValue.Trim();
            }
            else
            {
                Flags.Add(new FFlagEntry
                {
                    Name = NewFlagName.Trim(),
                    Value = NewFlagValue.Trim()
                });
            }

            NewFlagName = string.Empty;
            NewFlagValue = string.Empty;

            UpdateFilteredFlags();
            UpdateRawJsonText();
            DetectPresets();
        }

        [RelayCommand]
        private void DeleteFlag(FFlagEntry? entry)
        {
            if (entry == null)
                return;

            Flags.Remove(entry);
            UpdateFilteredFlags();
            UpdateRawJsonText();
            DetectPresets();
        }

        partial void OnSelectedFpsPresetChanged(string value)
        {
            const string fpsFlag = "DFIntTaskSchedulerTargetFps";

            if (value == "Default")
            {
                RemoveFlag(fpsFlag);
            }
            else
            {
                var val = value.StartsWith("Uncapped") ? "9999" : value;
                SetOrUpdateFlag(fpsFlag, val);
            }

            UpdateFilteredFlags();
            UpdateRawJsonText();
        }

        partial void OnSelectedGraphicsApiPresetChanged(string value)
        {
            const string d3d11Flag = "FFlagDebugGraphicsPreferD3D11";
            const string vulkanFlag = "FFlagDebugGraphicsPreferVulkan";
            const string openglFlag = "FFlagDebugGraphicsPreferOpenGL";

            RemoveFlag(d3d11Flag);
            RemoveFlag(vulkanFlag);
            RemoveFlag(openglFlag);

            if (value == "Direct3D11")
                SetOrUpdateFlag(d3d11Flag, "True");
            else if (value == "Vulkan")
                SetOrUpdateFlag(vulkanFlag, "True");
            else if (value == "OpenGL")
                SetOrUpdateFlag(openglFlag, "True");

            UpdateFilteredFlags();
            UpdateRawJsonText();
        }

        partial void OnSelectedMsaaPresetChanged(string value)
        {
            const string msaaFlag = "FIntDebugForceMSAASamples";

            if (value == "Default")
            {
                RemoveFlag(msaaFlag);
            }
            else
            {
                var val = value switch
                {
                    "Off (0)" => "0",
                    "2x" => "2",
                    "4x" => "4",
                    "8x" => "8",
                    _ => "0"
                };

                SetOrUpdateFlag(msaaFlag, val);
            }

            UpdateFilteredFlags();
            UpdateRawJsonText();
        }

        private void SetOrUpdateFlag(string name, string value)
        {
            var existing = Flags.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                existing.Value = value;
            else
                Flags.Add(new FFlagEntry { Name = name, Value = value });
        }

        private void RemoveFlag(string name)
        {
            var existing = Flags.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                Flags.Remove(existing);
        }

        private void DetectPresets()
        {
            var fpsFlag = Flags.FirstOrDefault(f => f.Name.Equals("DFIntTaskSchedulerTargetFps", StringComparison.OrdinalIgnoreCase));

            if (fpsFlag != null)
            {
                SelectedFpsPreset = fpsFlag.Value switch
                {
                    "60" => "60",
                    "120" => "120",
                    "144" => "144",
                    "240" => "240",
                    "360" => "360",
                    "9999" => "Uncapped (9999)",
                    _ => "Default"
                };
            }

            var msaaFlag = Flags.FirstOrDefault(f => f.Name.Equals("FIntDebugForceMSAASamples", StringComparison.OrdinalIgnoreCase));

            if (msaaFlag != null)
            {
                SelectedMsaaPreset = msaaFlag.Value switch
                {
                    "0" => "Off (0)",
                    "2" => "2x",
                    "4" => "4x",
                    "8" => "8x",
                    _ => "Default"
                };
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                if (SelectedTabIndex == 1)
                    ParseRawJsonToFlags();

                var dict = FlagsToDictionary();

                await Task.Run(() => _fflagManager.SaveFFlags(dict));

                CloseSpecificWindow<FFlagEditorWindow>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveAsync));
            }
        }
    }
}
