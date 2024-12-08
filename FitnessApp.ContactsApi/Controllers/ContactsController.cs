using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessApp.Contacts.Common.Models;
using FitnessApp.ContactsApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.ContactsApi.Controllers;

public class ContactsController(IContactsService contactsService) : Controller
{
    [HttpGet("CreateSavaAndHfed")]
    public async Task CreateSavaAndHfed()
    {
        await CreateUser("Igor", "Sava");

        // await CreateUser("Fedir", "Nedashkovskiy");
        // await CreateUser("Pedir", "Nedashkovskiy");
        // await CreateUser("Pedir", "Nedoshok");
        await CreateUser("Fehin", "Nedoshok");

        await CreateUser("Pehin", "Nedoshok");

        // await CreateUser("Pedtse", "Nedoshok");
        await CreateUser("Fedtse", "Nedoshok");

        // await CreateUser("Hfedir", "Nedoshok");
        // await CreateUser("Hfehin", "Nedoshok");
        // await CreateUser("Pedir", "Nedoshko");
        // await CreateUser("Fehin", "Nedoshko");
        // await CreateUser("Pehin", "Nedoshko");
        // await CreateUser("Pedtse", "Nedoshko");
        // await CreateUser("Fedtse", "Nedoshko");
        // await CreateUser("Hfedir", "Nedoshko");
        // await CreateUser("Hfehin", "Nedoshko");
    }

    [HttpGet("HfedFollowsSava")]
    public async Task HfedFollowsSava()
    {
        var sava = (await GetUsers("Sa")).Single();
        var fehin = (await GetUsers("Fe")).Find(feh => feh.FirstName == "Fehin" && feh.LastName == "Nedoshok");
        await contactsService.FollowUser(fehin.UserId, sava.UserId);
        var fedtse = (await GetUsers("Fe")).Find(feh => feh.FirstName == "Fedtse" && feh.LastName == "Nedoshok");
        await contactsService.FollowUser(fedtse.UserId, sava.UserId);
        var pehin = (await GetUsers("Pe")).Find(feh => feh.FirstName == "Pehin" && feh.LastName == "Nedoshok");
        await contactsService.FollowUser(pehin.UserId, sava.UserId);
    }

    [HttpGet("GetHfedThatFollowsSava")]
    public async Task<List<UserModel>> GetHfedThatFollowsSava()
    {
        var sava = (await GetUsers("Sa")).Single();
        var model = new GetUsersModel { Search = "f", Page = 0, PageSize = 10 };
        var users = await contactsService.GetUserFollowers(sava.UserId, model);
        return users.Items.ToList();
    }

    private async Task CreateUser(string firstName, string lastName)
    {
        await contactsService.AddUser(new UserModel
        {
            UserId = Guid.NewGuid().ToString("N"),
            FirstName = firstName,
            LastName = lastName,
        });
    }

    private async Task<List<UserModel>> GetUsers(string seatch)
    {
        var model = new GetUsersModel { Search = seatch, Page = 0, PageSize = 10 };
        var users = await contactsService.GetUsers(model);
        return [..users.Items];
    }
}
