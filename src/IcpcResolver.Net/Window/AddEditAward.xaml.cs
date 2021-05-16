using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
namespace IcpcResolver.Net.Window
{
    /// <summary>
    /// AddEditAward.xaml 的交互逻辑
    /// </summary>
    public partial class AddEditAward
    {
        public bool AwardInfoChanged;
        public List<String> ReturnedAward;
        public AddEditAward(string name, List<string> awards)
        {
            InitializeComponent();
            this.awardTeamName.Text = name;
            this.awardNames.Text = String.Join(";", awards).Trim(';') + ";";
            this.AwardInfoChanged = false;
            this.ReturnedAward = new List<string>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void awardNames_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.AwardInfoChanged = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.AwardInfoChanged)
            {
                foreach (var award in this.awardNames.Text.Trim(';').Split(';'))
                    this.ReturnedAward.Add(award);
            }
            this.Close();
        }
    }
}
