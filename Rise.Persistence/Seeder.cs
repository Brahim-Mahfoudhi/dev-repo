using Rise.Shared.Enums;
using Rise.Domain.Products;
using Rise.Domain.Users;

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
        if (!ProductsHasAlreadyBeenSeeded())
            SeedProducts();
        if (!UsersHasAlreadyBeenSeeded())
            SeedUsers();
    }

    /// <summary>
    /// Checks if the database has already been seeded with products.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the database already contains product entries; otherwise, <c>false</c>.
    /// </returns>
    private bool ProductsHasAlreadyBeenSeeded()
    {
        return dbContext.Products.Any();
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

    /// <summary>
    /// Seeds the database with a range of product entities.
    /// </summary>
    private void SeedProducts()
    {
        var products = Enumerable.Range(1, 20)
                                 .Select(i => new Product { Name = $"Product {i}" })
                                 .ToList();

        dbContext.Products.AddRange(products);
        dbContext.SaveChanges();
    }

    /// <summary>
    /// Seeds the database with 2 user entities.
    /// </summary>
    private void SeedUsers()
    {
        var userAdmin = new User("auth0|6713ad524e8a8907fbf0d57f","Admin", "Gebruiker", "admin@gmail.com",  new DateTime(1980, 01, 01), new Address("Afrikalaan", "5"), "+32478457845");
        userAdmin.AddRole(new Role(RolesEnum.Admin));
        dbContext.Users.Add(userAdmin);
        var userUser = new User("auth0|6713ad784fda04f4b9ae2165","GodParent", "Gebruiker", "godparent@gmail.com", new DateTime(1986, 09, 27), new Address("Bataviabrug", "35"), "+32478471869");
        userUser.AddRole(new Role());
        dbContext.Users.Add(userUser);
        var userGodparent = new User("auth0|6713ad614fda04f4b9ae2156","User", "Gebruiker", "user@gmail.com", new DateTime(1990, 05, 16), new Address("Deckerstraat", "4"), "+32474771836");
        userGodparent.AddRole(new Role(RolesEnum.Godparent));
        dbContext.Users.Add(userGodparent);
        var userPending = new User("auth0|6713adbf2d2a7c11375ac64c","Pending", "Gebruiker", "pending@gmail.com", new DateTime(1990, 05, 16), new Address("Deckerstraat", "4"), "+32474771836");
        userPending.AddRole(new Role(RolesEnum.Pending));
        dbContext.Users.Add(userPending);
        dbContext.SaveChanges();
    }
}

