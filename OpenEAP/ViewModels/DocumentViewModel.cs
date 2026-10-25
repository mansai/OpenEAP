using AtomUI.Controls;
using Avalonia.Controls;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI.Reactive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenEAP.ViewModels
{
    public class DocumentViewModel : Document
    {
        private UserControl _content;
        [JsonIgnore]
        public Icon Icon { get; set; }
        [JsonIgnore]
        public UserControl Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }
    }
}
