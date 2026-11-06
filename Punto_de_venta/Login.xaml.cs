using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Punto_de_venta
{
    public class User
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public sealed partial class Login : Window
    {
        private List<User> _users = new();

        public Login()
        {
            this.InitializeComponent();
            LoadUsersFromJson();
        }

        private void LoadUsersFromJson()
        {
            try
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, "users.json");
                string json = File.ReadAllText(filePath);
                _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                _users = new List<User>();
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            string usuario = UsernameTextBox.Text.Trim();
            string contrasena = PasswordBoxControl.Password.Trim();
            var user = _users.FirstOrDefault(u =>
                u.Username == usuario && u.Password == contrasena);

            if (user != null)
            {
                var menu = new Menu_principal(user);
                menu.Activate();
                this.Close();
            }
            else
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Usuario o contraseña incorrectos.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
