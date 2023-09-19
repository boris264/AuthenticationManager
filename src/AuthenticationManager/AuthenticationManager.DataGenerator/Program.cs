using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AuthenticationManager.Data.Models;
using System.Text.Json.Serialization;

namespace AuthenticationManager.DataGenerator
{
    internal class Program
    {
        private static string _emailsFilename = "Emails.txt";
        private static string _usernamesFilename = "Usernames.txt";
        private static string _passwordsFilename = "Passwords.txt";

        static async Task Main(string[] args)
        {
            List<string> emails = new List<string>(await File.ReadAllLinesAsync(_emailsFilename));
            List<string> usernames = new List<string>(await File.ReadAllLinesAsync(_usernamesFilename));
            List<string> passwords = new List<string>(await File.ReadAllLinesAsync(_passwordsFilename));

            var users = GenerateUsers(emails, usernames, passwords, 30);
            await ToJsonFile(users, "users.json");
        }

        private static async Task ToJsonFile<T>(List<T> data, string outputFile)
            where T : class
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = true,
            };

            try
            {
                using (Stream stream = File.Open(outputFile, FileMode.OpenOrCreate))
                {
                    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to add data to json file! Exception: {e.Message}");
            }
        }

        private static List<User> GenerateUsers(List<string> emailsList,
                                           List<string> usernamesList,
                                           List<string> passwordsList,
                                           int usersToGenerate)
        {
            List<User> generatedUsers = new List<User>();

            for (int i = 0; i < usersToGenerate; i++)
            {
                Random random = new Random();

                int emailRandomIndex = random.Next(emailsList.Count);
                int usernameRandomIndex = random.Next(usernamesList.Count);
                int passwordRandomIndex = random.Next(passwordsList.Count);

                User user = new User()
                {
                    Id = Guid.NewGuid(),
                    Email = emailsList[emailRandomIndex],
                    Username = usernamesList[usernameRandomIndex],
                    Password = passwordsList[passwordRandomIndex],
                    UserUserAgents = null,
                    UserConnections = null
                };

                user.UserClaims = new List<UserClaim>() 
                {
                    new UserClaim() 
                    { 
                        Claim = new Claim() 
                        { 
                            Id = Guid.NewGuid(), Name = "Username", Value = user.Username 
                        },
                        UserId = user.Id,
                        ClaimId = default
                    },
                    new UserClaim() 
                    { 
                        Claim = new Claim() 
                        { 
                            Id = Guid.NewGuid(), Name = "Email", Value = user.Email 
                        },
                        UserId = user.Id,
                        ClaimId = default
                    }
                };

                user.UserRoles = new List<UserRole>() 
                {
                    new UserRole() {
                        Role = new Role() 
                        { 
                            Id = Guid.NewGuid(), 
                            Name = random.Next(10) < 2 ? "Administrator" : "User"
                        },
                        UserId = user.Id,
                        RoleId = default
                    },
                };

                emailsList.RemoveAt(emailRandomIndex);
                usernamesList.RemoveAt(usernameRandomIndex);
                passwordsList.RemoveAt(passwordRandomIndex);

                generatedUsers.Add(user);
            }

            return generatedUsers;
        }
    }
}