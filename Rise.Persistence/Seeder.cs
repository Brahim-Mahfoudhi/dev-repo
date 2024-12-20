using Rise.Domain.Bookings;
using Rise.Shared.Enums;
using Rise.Domain.Users;
using Rise.Domain.Notifications;

namespace Rise.Persistence;

/// <summary>
/// Responsible for seeding the database with initial data.
/// </summary>
public class Seeder
{
    private readonly ApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="Seeder"/> class with a specified <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="dbContext">The database context used for seeding.</param>
    public Seeder(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Seeds the database with initial data if it has not been seeded already.
    /// </summary>
    public void Seed()
    {
        if (!BoatsHasAlreadyBeenSeeded())
        {
            DropBookings();
            SeedBoats();
        }

        if (!BatteriesHasAlreadyBeenSeeded())
        {
            DropBookings();
            SeedBatteries();
        }
        if (!UsersHasAlreadyBeenSeeded())
            SeedUsers();

        if (!BookingsHasAlreadyBeenSeeded())
            SeedBookings();

        if (!NotificationsHasAlreadyBeenSeeded())
            SeedNotifications();
    }

    /// <summary>
    /// Checks if the database has already been seeded with users.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the database already contains user entries; otherwise, <c>false</c>.
    /// </returns>
    private bool UsersHasAlreadyBeenSeeded()
    {
        return dbContext.Users.Any();
    }

    private bool BookingsHasAlreadyBeenSeeded()
    {
        return dbContext.Bookings.Any();
    }

    private bool BoatsHasAlreadyBeenSeeded()
    {
        return dbContext.Boats.Any();
    }

    private bool BatteriesHasAlreadyBeenSeeded()
    {
        return dbContext.Batteries.Any();
    }

    private bool NotificationsHasAlreadyBeenSeeded()
    {
        return dbContext.Notifications.Any();
    }

    private bool DropUsers()
    {
        dbContext.Users.RemoveRange(dbContext.Users.AsEnumerable());
        return true;
    }

    private bool DropBookings()
    {
        dbContext.Bookings.RemoveRange(dbContext.Bookings.AsEnumerable());
        return true;
    }

    private bool DropNotifications()
    {
        dbContext.Notifications.RemoveRange(dbContext.Notifications.AsEnumerable());
        return true;
    }

    /// <summary>
    /// Seeds the database with 2 user entities.
    /// </summary>
    private void SeedUsers()
    {
        var roleAdmin = new Role(RolesEnum.Admin);
        var roleUser = new Role(RolesEnum.User);
        var roleBUUTAgent = new Role(RolesEnum.BUUTAgent);
        var rolePending = new Role(RolesEnum.Pending);

        var userAdmin = new User("auth0|6713ad524e8a8907fbf0d57f", "Admin", "Gebruiker", "admin@hogent.be",
            new DateTime(1980, 01, 01), new Address("Afrikalaan", "5"), "+32478457845");
        var userBUUTAgent = new User("auth0|6713ad784fda04f4b9ae2165", "BUUTAgent", "Gebruiker", "BUUTAgent@hogent.be",
            new DateTime(1986, 09, 27), new Address("Bataviabrug", "35"), "+32478471869");
        var userUser = new User("auth0|6713ad614fda04f4b9ae2156", "User", "Gebruiker", "user@hogent.be",
            new DateTime(1990, 05, 16), new Address("Deckerstraat", "4"), "+32474771836");
        var userPending = new User("auth0|6713adbf2d2a7c11375ac64c", "Pending", "Gebruiker", "pending@hogent.be",
            new DateTime(1990, 05, 16), new Address("Deckerstraat", "4"), "+32474771836");

        userAdmin.Roles.Add(roleAdmin);
        userAdmin.Roles.Add(roleUser);
        userUser.Roles.Add(roleUser);
        userBUUTAgent.Roles.Add(roleBUUTAgent);
        userBUUTAgent.Roles.Add(roleUser);
        userPending.Roles.Add(rolePending);
        dbContext.Users.AddRange(userAdmin, userUser, userBUUTAgent, userPending);
        dbContext.Roles.AddRange(roleAdmin, roleUser, roleBUUTAgent, rolePending);
        dbContext.SaveChanges();
    }

    private void SeedBookings()
    {
        // // temp seed bookings (Andries)
        var bookings = new List<Booking>
        {
            new Booking(new DateTime(2023, 01, 01), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning),
            new Booking(new DateTime(2023, 01, 02), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Evening),
            new Booking(new DateTime(2023, 01, 03), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Afternoon),
            new Booking(new DateTime(2023, 01, 04), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Afternoon),
            new Booking(new DateTime(2023, 01, 05), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Evening),
            new Booking(new DateTime(2023, 01, 06), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Evening),
            new Booking(new DateTime(2023, 01, 07), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning),
            new Booking(new DateTime(2023, 01, 08), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Afternoon),
            new Booking(new DateTime(2023, 01, 09), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Evening),
            new Booking(new DateTime(2023, 01, 10), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning)
        };

        foreach (var booking in bookings)
        {
            dbContext.Bookings.Add(booking);
        }
        dbContext.SaveChanges();

        var booking1 = new Booking(new DateTime(2025, 01, 01), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning);
        dbContext.Bookings.Add(booking1);
        var bookingBattery = new Booking(new DateTime(2023, 01, 01), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning);
        bookingBattery.AddBattery(dbContext.Batteries.First());
        dbContext.Bookings.Add(bookingBattery);
        var bookingBoat = new Booking(new DateTime(2022, 01, 01), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning);
        bookingBoat.AddBoat(dbContext.Boats.First());
        dbContext.Bookings.Add(bookingBoat);
        var bookingAll = new Booking(new DateTime(2021, 01, 01), "auth0|6713ad614fda04f4b9ae2156", TimeSlot.Morning);
        bookingAll.AddBattery(dbContext.Batteries.OrderBy(battery => battery.Name).Last());
        bookingAll.AddBoat(dbContext.Boats.OrderBy(boat => boat.Name).Last());
        dbContext.Bookings.Add(bookingAll);
        dbContext.SaveChanges();
    }

    private void SeedBoats()
    {
        for (int i = 1; i <= 3; i++)
        {
            dbContext.Boats.Add(new Boat("Boat" + i));
        }
        dbContext.SaveChanges();
    }

    private void SeedBatteries()
    {
        for (int i = 1; i <= 10; i++)
        {
            dbContext.Batteries.Add(new Battery("Battery" + i));
        }
        dbContext.SaveChanges();
    }

    private void SeedNotifications()
    {
        // var notifications = new List<Notification>
        // {
        //     // Notifications for User
        //     new Notification(
        //         userId: "auth0|6713ad614fda04f4b9ae2156",
        //         title_EN: "Booking Reminder",
        //         title_NL: "Herinnering voor Boeking",
        //         message_EN: "Don't forget about your upcoming booking tomorrow.",
        //         message_NL: "Vergeet niet uw komende boeking morgen.",
        //         type: NotificationType.Reminder
        //     ),
        //     new Notification(
        //         userId: "auth0|6713ad614fda04f4b9ae2156",
        //         title_EN: "Welcome to our service",
        //         title_NL: "Welkom bij onze dienst",
        //         message_EN: "We are glad to have you here.",
        //         message_NL: "We zijn blij dat u hier bent.",
        //         type: NotificationType.General
        //     ),
        //     // Notifications for Admin
        //     new Notification(
        //         userId: "auth0|6713ad524e8a8907fbf0d57f",
        //         title_EN: "New User Registration",
        //         title_NL: "Nieuwe Gebruikersregistratie",
        //         message_EN: "A new user has just registered.",
        //         message_NL: "Een nieuwe gebruiker heeft zich zojuist geregistreerd.",
        //         type: NotificationType.UserRegistration
        //     ),
        //     new Notification(
        //         userId: "auth0|6713ad524e8a8907fbf0d57f",
        //         title_EN: "System Maintenance",
        //         title_NL: "Systeemonderhoud",
        //         message_EN: "Scheduled maintenance will occur tonight.",
        //         message_NL: "Gepland onderhoud vindt vanavond plaats.",
        //         type: NotificationType.Alert
        //     ),
        //     // Notifications for BUUTAgent
        //     new Notification(
        //         userId: "auth0|6713ad784fda04f4b9ae2165",
        //         title_EN: "Battery Check",
        //         title_NL: "Batterijcontrole",
        //         message_EN: "Please check the batteries before your next shift.",
        //         message_NL: "Controleer de batterijen voor uw volgende dienst.",
        //         type: NotificationType.Reminder
        //     )
        // };
        var notifications = new List<Notification>
    {
        // Notifications for User
        new Notification(
            userId: "auth0|6713ad614fda04f4b9ae2156",
            title_EN: "Booking Reminder",
            title_NL: "Herinnering voor Boeking",
            message_EN: "Don't forget about your upcoming booking tomorrow.",
            message_NL: "Vergeet niet uw komende boeking morgen.",
            type: NotificationType.Booking,
            relatedEntityId: dbContext.Bookings.FirstOrDefault(b => b.UserId == "auth0|6713ad614fda04f4b9ae2156")?.Id
        ),
        new Notification(
            userId: "auth0|6713ad614fda04f4b9ae2156",
            title_EN: "Welcome to our service",
            title_NL: "Welkom bij onze dienst",
            message_EN: "We are glad to have you here.",
            message_NL: "We zijn blij dat u hier bent.",
            type: NotificationType.General
        ),
        new Notification(
            userId: "auth0|6713ad614fda04f4b9ae2156",
            title_EN: "Scheduled Maintenance",
            title_NL: "Gepland Onderhoud",
            message_EN: "The system will undergo maintenance tonight. Expect some downtime.",
            message_NL: "Het systeem zal vanavond onderhoud ondergaan. Verwacht enige downtime.",
            type: NotificationType.Alert
        ),

        // Notifications for Admin
        new Notification(
            userId: "auth0|6713ad524e8a8907fbf0d57f",
            title_EN: "New User Registration",
            title_NL: "Nieuwe Gebruikersregistratie",
            message_EN: "A new user has just registered.",
            message_NL: "Een nieuwe gebruiker heeft zich zojuist geregistreerd.",
            type: NotificationType.UserRegistration,
            relatedEntityId: dbContext.Users.FirstOrDefault(u => u.Email == "user@hogent.be")?.Id
        ),
        new Notification(
            userId: "auth0|6713ad524e8a8907fbf0d57f",
            title_EN: "System Maintenance",
            title_NL: "Systeemonderhoud",
            message_EN: "Scheduled maintenance will occur tonight.",
            message_NL: "Gepland onderhoud vindt vanavond plaats.",
            type: NotificationType.Alert
        ),
        new Notification(
            userId: "auth0|6713ad524e8a8907fbf0d57f",
            title_EN: "New Booking Created",
            title_NL: "Nieuwe Boeking Gemaakt",
            message_EN: "A new booking has been created by 'User456'.",
            message_NL: "Een nieuwe boeking is gemaakt door 'User456'.",
            type: NotificationType.Booking,
            relatedEntityId: dbContext.Bookings.FirstOrDefault(b => b.UserId == "auth0|6713ad614fda04f4b9ae2156")?.Id
        ),

        // Notifications for BUUTAgent
        new Notification(
            userId: "auth0|6713ad784fda04f4b9ae2165",
            title_EN: "Battery Check",
            title_NL: "Batterijcontrole",
            message_EN: "Please check the batteries before your next shift.",
            message_NL: "Controleer de batterijen voor uw volgende dienst.",
            type: NotificationType.Battery,
            relatedEntityId: dbContext.Batteries.FirstOrDefault()?.Id
        ),
        new Notification(
            userId: "auth0|6713ad784fda04f4b9ae2165",
            title_EN: "Scheduled Maintenance",
            title_NL: "Gepland Onderhoud",
            message_EN: "Remember to check equipment after the scheduled maintenance.",
            message_NL: "Vergeet niet de apparatuur te controleren na het geplande onderhoud.",
            type: NotificationType.Alert
        ),
        new Notification(
            userId: "auth0|6713ad784fda04f4b9ae2165",
            title_EN: "Battery location",
            title_NL: "Batterijlocatie",
            message_EN: "The battery has been handed to User Gebruiker.",
            message_NL: "De batterij is overhandigd aan User Gebruiker.",
            type: NotificationType.Boat,
            relatedEntityId: dbContext.Boats.FirstOrDefault()?.Id
        ),
        new Notification(
            userId: "auth0|6713ad784fda04f4b9ae2165",
            title_EN: "Boat Inspection",
            title_NL: "Bootinspectie",
            message_EN: "Please perform a boat inspection.",
            message_NL: "Voer een bootinspectie uit.",
            type: NotificationType.Boat,
            relatedEntityId: dbContext.Boats.FirstOrDefault()?.Id
        )
    };


        dbContext.Notifications.AddRange(notifications);
        dbContext.SaveChanges();
    }
}