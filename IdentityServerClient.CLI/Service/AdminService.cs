using IdentityServ.Models;
using IdentityServ.Rules;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IdentityServerClient.CLI.Service
{
	public class AdminService
	{
		public static async System.Threading.Tasks.Task<int> AddAsync(Options.AddAdminOptions options)
		{
			ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
			});
			ILogger logger = loggerFactory.CreateLogger<AdminService>();
			var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>();
			using (var ctx = new ApplicationDbContext(contextOptions.UseSqlServer(options.DatabaseConnection).Options))
			{
				using (var roleStore = new RoleStore<IdentityRole>(ctx))
				{
					using (var roleManager = new RoleManager<IdentityRole>(roleStore, roleValidators: null, keyNormalizer: null, errors: null, logger: loggerFactory.CreateLogger<RoleManager<IdentityRole>>()))
					{
						var role = await roleManager.FindByNameAsync("Admin");
						if (role == null)
						{
							var createRoleResponse = await roleManager.CreateAsync(new IdentityRole("Admin"));
							if (!createRoleResponse.Succeeded) throw new Exception(string.Join(Environment.NewLine, createRoleResponse.Errors.Select(x => x.Description)));
						}
					}
				}
			}

			using (var ctx = new ApplicationDbContext(contextOptions.UseSqlServer(options.DatabaseConnection).Options))
			{
				using (var userStore = new UserStore<ApplicationUser>(ctx, null))
				{
					using (var userManager = new UserManager<ApplicationUser>(userStore,
						optionsAccessor: null,
						passwordHasher: new PasswordHasher<ApplicationUser>(),
						userValidators: null,
						passwordValidators: null,
						keyNormalizer: null,
						errors: null,
						services: null,
						logger: loggerFactory.CreateLogger<UserManager<ApplicationUser>>()))
					{
						ApplicationUser user = await userManager.FindByEmailAsync(options.Email);
						if (user == null)
						{
							Console.WriteLine("User doesn't exist. Please enter username to create a new user");
							Console.Write("Username: ");
							var username = Console.ReadLine();
							Console.Write("Password: ");
							var pass = string.Empty;
							ConsoleKey key;
							do
							{
								var keyInfo = Console.ReadKey(intercept: true);
								key = keyInfo.Key;

								if (key == ConsoleKey.Backspace && pass.Length > 0)
								{
									Console.Write("\b \b");
									pass = pass[0..^1];
								}
								else if (!char.IsControl(keyInfo.KeyChar))
								{
									Console.Write("*");
									pass += keyInfo.KeyChar;
								}
							} while (key != ConsoleKey.Enter);
							Console.WriteLine();
							var newUser = new ApplicationUser()
							{
								Email = options.Email,
								UserName = username
							};
							var addUserResponse = await userManager.CreateAsync(newUser, pass);
							if (!addUserResponse.Succeeded) throw new Exception(string.Join(Environment.NewLine, addUserResponse.Errors.Select(x => x.Description)));
							user = await userManager.FindByEmailAsync(options.Email);
						}
						if(!await userManager.IsInRoleAsync(user,"Admin"))
						{
							var addRoleResponse = await userManager.AddToRolesAsync(user, new string[] { "Admin" });
							if (!addRoleResponse.Succeeded) throw new Exception(string.Join(Environment.NewLine, addRoleResponse.Errors.Select(x => x.Description)));
						}
						

					}
				}
			}
			Console.WriteLine($"Added {options.Email} to Admin Role");
			return 0;
		}
	}
}
