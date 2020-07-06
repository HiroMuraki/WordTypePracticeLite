using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WordTypePracticeLite {
    static class ResDict {
        static public readonly ResourceDictionary PreSetting = new ResourceDictionary() {
            Source = new Uri("Styles/PreSetting.xaml", UriKind.Relative)
        };
    }
}
