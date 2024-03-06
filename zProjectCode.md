
## Folder Structure

```yml
│   aspnetrun-microservices.sln
│   docker-compose.override.yml
│   docker-compose.yml
│
└───Services
    ├───Basket
    │   └───Basket.API
    │       │   appsettings.json
    │       │   Basket.API.csproj
    │       │   Dockerfile
    │       │   Program.cs
    │       │   Startup.cs
    │       │
    │       ├───Controllers
    │       │       BasketController.cs
    │       │
    │       ├───Entities
    │       │       ShoppingCart.cs
    │       │       ShoppingCartItem.cs
    │       │
    │       ├───GrpcServices
    │       │       DiscountGrpcService.cs
    │       │
    │       └───Repositories
    │               BasketRepository.cs
    │               IBasketRepository.cs
    │
    ├───Catalog
    │   └───Catalog.API
    │       │   appsettings.json
    │       │   Catalog.API.csproj
    │       │   Dockerfile
    │       │   Program.cs
    │       │   Startup.cs
    │       │
    │       ├───Controllers
    │       │       CatalogController.cs
    │       │
    │       ├───Data
    │       │       CatalogContext.cs
    │       │       CatalogContextSeed.cs
    │       │       ICatalogContext.cs
    │       │
    │       ├───Entities
    │       │       Product.cs
    │       │
    │       └───Repositories
    │               IProductRepository.cs
    │               ProductRepository.cs
    │
    ├───Discount
    │   ├───Discount.API
    │   │   │   appsettings.json
    │   │   │   Discount.API.csproj
    │   │   │   Dockerfile
    │   │   │   Program.cs
    │   │   │   Startup.cs
    │   │   │
    │   │   ├───Controllers
    │   │   │       DiscountController.cs
    │   │   │
    │   │   ├───Entities
    │   │   │       Coupon.cs
    │   │   │
    │   │   ├───Extensions
    │   │   │       HostExtensions.cs
    │   │   │
    │   │   └───Repositories
    │   │           DiscountRepository.cs
    │   │           IDiscountRepository.cs
    │   │
    │   └───Discount.Grpc
    │       │   appsettings.json
    │       │   Discount.Grpc.csproj
    │       │   Dockerfile
    │       │   Program.cs
    │       │   Startup.cs
    │       │
    │       ├───Entities
    │       │       Coupon.cs
    │       │
    │       ├───Extensions
    │       │       HostExtensions.cs
    │       │
    │       ├───Mapper
    │       │       DiscountProfile.cs
    │       │
    │       ├───Protos
    │       │       discount.proto
    │       │
    │       ├───Repositories
    │       │       DiscountRepository.cs
    │       │       IDiscountRepository.cs
    │       │
    │       └───Services
    │               DiscountService.cs
    │
    └───Ordering
        ├───Ordering.API #------------------------------------------------------> Ordering.Application, Ordering.Infrastructure
        │   │   appsettings.json
        │   │   Dockerfile
        │   │   Ordering.API.csproj
        │   │   Program.cs
        │   │   Startup.cs # calls AddApplicationServices (Ordering.Application) and AddInfrastructureServices (Ordering.Infrastructure)
        │   │
        │   ├───Controllers
        │   │       OrderController.cs
        │   │
        │   ├───Extensions
        │          HostExtensions.cs
        │
        ├───Ordering.Application #-------------------------------------------> Ordering.Domain
        │   │   ApplicationServiceRegistration.cs
        │   │   Ordering.Application.csproj # FluentValidation.DependencyInjectionExtensions, MediatR.Extensions.Microsoft.DependencyInjection, 
        │   │                               # AutoMapper.Extensions.Microsoft.DependencyInjection
        │   ├───Behaviours
        │   │       UnhandledExceptionBehaviour.cs
        │   │       ValidationBehaviour.cs
        │   │
        │   ├───Contracts
        │   │   ├───Infrastructure
        │   │   │       IEmailService.cs
        │   │   │
        │   │   └───Persistence
        │   │           IAsyncRepository.cs
        │   │           IOrderRepository.cs
        │   │
        │   ├───Exceptions
        │   │       NotFoundException.cs
        │   │       ValidationException.cs
        │   │
        │   ├───Features
        │   │   └───Orders
        │   │       ├───Commands
        │   │       │   ├───CheckoutOrder
        │   │       │   │       CheckoutOrderCommand.cs
        │   │       │   │       CheckoutOrderCommandHandler.cs
        │   │       │   │       CheckoutOrderCommandValidator.cs
        │   │       │   │
        │   │       │   ├───DeleteOrder
        │   │       │   │       DeleteOrderCommand.cs
        │   │       │   │       DeleteOrderCommandHandler.cs
        │   │       │   │
        │   │       │   └───UpdateOrder
        │   │       │           UpdateOrderCommand.cs
        │   │       │           UpdateOrderCommandHandler.cs
        │   │       │           UpdateOrderCommandValidator.cs
        │   │       │
        │   │       └───Queries
        │   │           └───GetOrdersList
        │   │                   GetOrdersListQuery.cs
        │   │                   GetOrdersListQueryHandler.cs
        │   │                   OrdersVm.cs
        │   │
        │   ├───Mappings
        │   │       MappingProfile.cs
        │   │
        │   └───Models
        │           Email.cs
        │           EmailSettings.cs
        │    
        ├───Ordering.Domain # <<<<-----------------------------------Ordering.Domain doesn't reference anything
        │   │   Ordering.Domain.csproj # empty
        │   │
        │   ├───Common
        │   │       EntityBase.cs
        │   │       ValueObject.cs
        │   │
        │   └───Entities
        │           Order.cs
        │
        └───Ordering.Infrastructure  #-----------------------------------> Ordering.Application
            │   InfrastructureServiceRegistration.cs
            │   Ordering.Infrastructure.csproj # Microsoft.EntityFrameworkCore.SqlServer, SendGrid
            │
            ├───Mail
            │       EmailService.cs
            │
            ├───Migrations
            │       20240201082214_InitialCreate.cs
            │       20240201082214_InitialCreate.Designer.cs
            │       OrderContextModelSnapshot.cs
            │
            ├───Persistence
            │       OrderContext.cs
            │       OrderContextSeed.cs
            │
            └───Repositories
                    OrderRepository.cs
                    RepositoryBase.cs
```



## Ordering Projects Code

From `Ordering` folder


`Ordering.API`

```C#
//------------------V
public class Startup
{
    // ..
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices(); // <------------------------defined in Ordering.Application's ApplicationServiceRegistration.cs
        /* above extension method mainly do:
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));  
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
       
        UnhandledExceptionBehaviour and ValidationBehaviour defined in Ordering.Application\Behaviours folder
        */

        services.AddInfrastructureServices(Configuration);  // <------------------------defined in Ordering.Infrastructure

        // ...
    }
    // ...
}
//------------------Ʌ

//------------------V
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args)
            .Build()
            .MigrateDatabase<OrderContext>((context, services) =>  // <--------------------OrderContext and OrderContextSeed from Ordering.Infrastructure
            {
                var logger = services.GetService<ILogger<OrderContextSeed>>();
                OrderContextSeed
                    .SeedAsync(context, logger)
                    .Wait();
            })
            .Run();
    }
    // ...
}
//------------------Ʌ

//--------------------------------V
public static class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder, int retry = 0) where TContext : DbContext
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();  // <------------------Get OrderDbContext

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                //
                context.Database.Migrate();
                seeder(context, services);
                //

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

                if (retry < 5)
                {
                    retry++;
                    System.Threading.Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, seeder, retry);
                }
            }
        }
        return host;
    }
}
//--------------------------------Ʌ
```


`Ordering.Application` (Class Library)

```C#
//------------------------------------------------V
public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
//------------------------------------------------Ʌ

//-----------------------------------------------------------V Behaviours 
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "Application Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
            throw;
        }
    }
}
//-----------------------------------------------------------Ʌ

//---------------------------------------------------V Behaviours
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>  // IPipelineBehavior from MediatR
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).ToList();

            if (failures.Any())
                throw new ValidationException(failures);
        }

        return await next();
    }
}
//---------------------------------------------------Ʌ

//----------------------------V Contracts\Infrastructure
public interface IEmailService
{
    Task<bool> SendEmail(Email email);
}
//----------------------------Ʌ

//-------------------------------V Contracts\Persistence
public interface IOrderRepository : IAsyncRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserName(string userName);
}
//-------------------------------Ʌ

//----------------------------------V Contracts\Persistence
public interface IAsyncRepository<T> where T : EntityBase
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                    string includeString = null,
                                    bool disableTracking = true);
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null,
                                   Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                   List<Expression<Func<T, object>>> includes = null,
                                   bool disableTracking = true);
    Task<T> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
//----------------------------------Ʌ

// Features folder contains CQRS related files

//-------------------------------V Features\Orders\Commands\CheckoutOrder
public class CheckoutOrderCommand : IRequest<int>
{
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }

    // BillingAddress
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    // Payment
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string Expiration { get; set; }
    public string CVV { get; set; }
    public int PaymentMethod { get; set; }
}
//-------------------------------Ʌ

//--------------------------------------V Features\Orders\Commands\CheckoutOrder
public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<CheckoutOrderCommandHandler> _logger;

    public CheckoutOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService, ILogger<CheckoutOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<int> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = _mapper.Map<Order>(request);
        var newOrder = await _orderRepository.AddAsync(orderEntity);

        _logger.LogInformation($"Order {newOrder.Id} is successfully created.");

        await SendEmail(newOrder);

        return newOrder.Id;
    }

    private async Task SendEmail(Order order)
    {
        var email = new Email() { To = "ezozkme@gmail.com", Body = $"Order was created.", Subject = "Order was created" };

        try
        {
            await _emailService.SendEmail(email);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Order {order.Id} failed due to an error with the mail service: {ex.Message}");
        }
    }
}
//--------------------------------------Ʌ

//----------------------------------------V Features\Orders\Commands\CheckoutOrder
public class CheckoutOrderCommandValidator : AbstractValidator<CheckoutOrderCommand>
{
    public CheckoutOrderCommandValidator()
    {
        RuleFor(p => p.UserName)
            .NotEmpty().WithMessage("{UserName} is required.")
            .NotNull()
            .MaximumLength(50).WithMessage("{UserName} must not exceed 50 characters.");

        RuleFor(p => p.EmailAddress)
           .NotEmpty().WithMessage("{EmailAddress} is required.");

        RuleFor(p => p.TotalPrice)
            .NotEmpty().WithMessage("{TotalPrice} is required.")
            .GreaterThan(0).WithMessage("{TotalPrice} should be greater than zero.");
    }
}
//----------------------------------------Ʌ

// ... Features\Orders\Commands\DeleteOrder\* , Features\Orders\Commands\UpdateOrder\*

//-----------------------------V Features\Orders\Queries\GetOrdersList
public class GetOrdersListQuery : IRequest<List<OrdersVm>>
{
    public string UserName { get; set; }

    public GetOrdersListQuery(string userName)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
    }
}
//-----------------------------Ʌ

//------------------------------------V Features\Orders\Queries\GetOrdersList
public class GetOrdersListQueryHandler : IRequestHandler<GetOrdersListQuery, List<OrdersVm>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersListQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<OrdersVm>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Order> orderList = await _orderRepository.GetOrdersByUserName(request.UserName);
        return _mapper.Map<List<OrdersVm>>(orderList);
    }
}
//------------------------------------Ʌ

//-------------------V Features\Orders\Queries\GetOrdersList
public class OrdersVm
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }

    // BillingAddress
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    // Payment
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string Expiration { get; set; }
    public string CVV { get; set; }
    public int PaymentMethod { get; set; }
}
//-------------------Ʌ

//-------------------------V Mappings
public class MappingProfile : Profile  // Profile from AutoMapper
{
    public MappingProfile()
    {
        CreateMap<Order, OrdersVm>().ReverseMap();
        CreateMap<Order, CheckoutOrderCommand>().ReverseMap();
        CreateMap<Order, UpdateOrderCommand>().ReverseMap();
    }
}
//-------------------------Ʌ

//----------------V Models
public class Email
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
//----------------Ʌ

//------------------------V Models
public class EmailSettings
{
    public string ApiKey { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
}
//------------------------Ʌ
```


`Ordering.Domain`

```C#
//------------------------------V Common
public abstract class EntityBase
{
    public int Id { get; protected set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
//------------------------------Ʌ

//-------------------------------V Common
public abstract class ValueObject
{
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }

        return left?.Equals(right) != false;
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}
//-------------------------------Ʌ

//----------------V Entities
public class Order : EntityBase
{
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }

    // BillingAddress
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    // Payment
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string Expiration { get; set; }
    public string CVV { get; set; }
    public int PaymentMethod { get; set; }
}
//----------------Ʌ
```


`Ordering.Infrastructure`

```C#
//---------------------------------------------------V
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderContext>(options =>
           options.UseSqlServer(configuration.GetConnectionString("OrderingConnectionString")));

        services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.Configure<EmailSettings>(c => configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
//---------------------------------------------------Ʌ

//-----------------------V Mail
public class EmailService : IEmailService
{
    public EmailSettings _emailSettings { get; }
    public ILogger<EmailService> _logger { get; }

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmail(Email email)
    {
        var client = new SendGridClient(_emailSettings.ApiKey);

        var subject = email.Subject;
        var to = new EmailAddress(email.To);
        var emailBody = email.Body;

        var from = new EmailAddress
        {
            Email = _emailSettings.FromAddress,
            Name = _emailSettings.FromName
        };

        var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);
        var response = await client.SendEmailAsync(sendGridMessage);

        _logger.LogInformation("Email sent.");

        if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK)
            return true;

        _logger.LogError("Email sending failed.");
        return false;
    }
}
//-----------------------Ʌ

// Migrations\*

//-----------------------V Persistence
public class OrderContext : DbContext
{
    public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.Now;
                    entry.Entity.CreatedBy = "swn";
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = DateTime.Now;
                    entry.Entity.LastModifiedBy = "swn";
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
//-----------------------Ʌ

//---------------------------V Persistence
public class OrderContextSeed
{
    public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
    {
        if (!orderContext.Orders.Any())
        {
            orderContext.Orders.AddRange(GetPreconfiguredOrders());
            await orderContext.SaveChangesAsync();
            logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
        }
    }

    private static IEnumerable<Order> GetPreconfiguredOrders()
    {
        return new List<Order>
        {
            new Order() {UserName = "swn", FirstName = "Mehmet", LastName = "Ozkaya", EmailAddress = "ezozkme@gmail.com", AddressLine = "Bahcelievler", Country = "Turkey", TotalPrice = 350 }
        };
    }
}
//---------------------------Ʌ

//----------------------------V Repositories
public class RepositoryBase<T> : IAsyncRepository<T> where T : EntityBase
{
    protected readonly OrderContext _dbContext;

    public RepositoryBase(OrderContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbContext.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeString = null, bool disableTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();
        if (disableTracking) query = query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(includeString))
            query = query.Include(includeString);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            return await orderBy(query).ToListAsync();

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, List<Expression<Func<T, object>>> includes = null, bool disableTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (disableTracking)
            query = query.AsNoTracking();

        if (includes != null)
            query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null) query = query.Where(predicate);

        if (orderBy != null)
            return await orderBy(query).ToListAsync();

        return await query.ToListAsync();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        _dbContext.Set<T>().Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);

        await _dbContext.SaveChangesAsync();
    }
}
//----------------------------Ʌ

//--------------------------V Repositories
public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(OrderContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<Order>> GetOrdersByUserName(string userName)
    {
        var orderList = await _dbContext
            .Orders
            .Where(o => o.UserName == userName)
            .ToListAsync();

        return orderList;
    }
}
//--------------------------Ʌ
```