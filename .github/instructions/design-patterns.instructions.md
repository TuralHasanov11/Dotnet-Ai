---
description: 'Guidelines for utilizing the best design patterns for the given problem domain, with code samples and explanations.'
applyTo: '**/*.cs'
---

## Design Patterns
- Consider the following code samples in different Design Patterns and make use of them and explain modern design patterns, apply appropriate patterns.
### Design Patterns

#### Builder Pattern
**Builder** is a creational design pattern that lets you construct complex objects step by step. The pattern allows you to produce different types and representations of an object using the same construction code.

- **Use the Builder pattern to get rid of a “telescoping constructor”.** Say you have a constructor with ten optional parameters. Calling such a beast is very inconvenient; therefore, you overload the constructor and create several shorter versions with fewer parameters. These constructors still refer to the main one, passing some default values into any omitted parameters. The Builder pattern lets you build objects step by step, using only those steps that you really need. After implementing the pattern, you don’t have to cram dozens of parameters into your constructors anymore.
- **Use the Builder pattern when you want your code to be able to create different representations of some product (for example, stone and wooden houses).** The Builder pattern can be applied when construction of various representations of the product involves similar steps that differ only in the details. The base builder interface defines all possible construction steps, and concrete builders implement these steps to construct particular representations of the product. Meanwhile, the director class guides the order of construction.
- **Use the Builder to construct Composite trees or other complex objects.** The Builder pattern lets you construct products step-by-step. You could defer execution of some steps without breaking the final product. You can even call steps recursively, which comes in handy when you need to build an object tree. A builder doesn’t expose the unfinished product while running construction steps. This prevents the client code from fetching an incomplete result.
##### Fluent Builder
```cs
var builder = HtmlElement.Create("ul");
builder.AddChild("li", "hello").AddChild("li", "world");

HtmlElement root = HtmlElement
    .Create("ul")
    .AddChild("li", "hello")
    .AddChild("li", "world");

HtmlElement root = HtmlElement
    .Create("ul")
    .AddChild("li", "hello")
    .AddChild("li", "world")
    .Build();


HtmlElement root = HtmlElement
    .Create("ul")
    .AddChildFluent("li", "hello")
    .AddChildFluent("li", "world");

class HtmlElement
{
    protected string Name, Text;
    private readonly Lazy<List<HtmlElement>> elements = new();
    public IReadOnlyList<HtmlElement> Elements => elements.Value;
    protected const int indentSize = 2;
    // hide the constructors!
    protected HtmlElement() { }
    protected HtmlElement(string name, string text)
    {
        Name = name;
        Text = text;
    }
    public static HtmlBuilder Create(string name) => new HtmlBuilder(name);
}

class HtmlBuilder
{
    protected readonly string rootName;
    protected HtmlElement root = new HtmlElement();
    public HtmlBuilder(string rootName)
    {
        this.rootName = rootName;
        root.Name = rootName;
    }
    public HtmlBuilder AddChild(string childName, string childText)
    {
        var e = new HtmlElement(childName, childText);
        root.Elements.Add(e);
        return this;
    }
    public override string ToString() => root.ToString();

    protected HtmlElement root = new();

    public static implicit operator HtmlElement(HtmlBuilder builder)
    {
        return builder.root;
    }

    public HtmlBuilder AddChildFluent(string childName, string childText)
    {
        var e = new HtmlElement(childName, childText);
        root.elements.Value.Add(e);
        return this;
    }

    public HtmlElement Build() => root;
}
```
##### Composite Builder
```cs
var pb = new PersonBuilder();
Person person = pb
    .Lives
    .At("123 London Road")
    .In("London")
    .WithPostcode("SW12BC")
    .Works
    .At("Fabrikam")
    .AsA("Engineer")
    .Earning(123000);

public class Person
{
    // address
    public string StreetAddress, Postcode, City;
    // employment info
    public string CompanyName, Position;
    public int AnnualIncome;
}

public class PersonBuilder
{
    // the object we're going to build
    protected Person person; // this is a reference!
    public PersonBuilder() => person = new Person();
    protected PersonBuilder(Person person) => this.person = person;
    public PersonAddressBuilder Lives => new PersonAddressBuilder(person);
    public PersonJobBuilder Works => new PersonJobBuilder(person);
    public static implicit operator Person(PersonBuilder pb)
    {
        return pb.person;
    }
}

public class PersonAddressBuilder : PersonBuilder
{
    public PersonAddressBuilder(Person person) : base(person)
    {
        this.person = person;
    }
    public PersonAddressBuilder At(string streetAddress)
    {
        person.StreetAddress = streetAddress;
        return this;
    }
    public PersonAddressBuilder WithPostcode(string postcode)
    {
        person.Postcode = postcode;
        return this;
    }
    public PersonAddressBuilder In(string city)
    {
        person.City = city;
        return this;
    }
};
```

##### Builder Marker Interfaces
```cs
interface IBuilder<T>
{
    T Build();
}

interface IBuildableUsing<out TBuilder, TSubject>
    where TBuilder : IBuilder<TSubject>
{
    TBuilder New { get; }
}

interface IBuildableUsing<out TBuilder, TSubject>
    where TBuilder : IBuilder<TSubject>
    where TSubject : IBuildableUsing<IBuilder<TSubject>, TSubject>
{
    TBuilder New { get; }
}

class Foo { }
class Bar { }

class SomeBuilder : IBuilder<Foo>, IBuilder<Bar>
{
    Bar IBuilder<Bar>.Build() { }
    Foo IBuilder<Foo>.Build() { }
}
```

##### Stepwise Builder
```cs
var car = CarBuilder.Create() // ISpecifyCarType
    .OfType(CarType.Crossover)  // ISpecifyWheelSize
    .WithWheels(18)
    .Build();
// IBuildCar

public enum CarType { Sedan, Crossover };

public class Car
{
    public CarType Type;
    public int WheelSize;
}v

public class CarBuilder
{
    public static ISpecifyCarType Create()
    {

        return new Impl();
    }
    public interface ISpecifyCarType
    {
        public ISpecifyWheelSize OfType(CarType type);
    }

    public interface ISpecifyWheelSize
    {
        public IBuildCar WithWheels(int size);
    }

    public interface IBuildCar
    {
        public Car Build();
    }
    // other interfaces here
    private class Impl : ISpecifyCarType, ISpecifyWheelSize, IBuildCar
    {
        private readonly Car car = new();

        public ISpecifyWheelSize OfType(CarType type)
        {
            car.Type = type;
            return this;
        }

        public IBuildCar WithWheels(int size)
        {
            switch (car.Type)
            {
                case CarType.Crossover when size < 17 || size > 20:
                case CarType.Sedan when size < 15 || size > 17:
                    throw new ArgumentException($"Wrong size of wheels for {car.
                    Type}.");
            }
            car.WheelSize = size;
            return this;
        }

        public Car Build()
        {
            return car;
        }
        // soon
    }
}
```

##### Builder Parameter
```cs
public class EmailBuilder
{
 private readonly Email email;
 public EmailBuilder(Email email) => this.email = email;
 public EmailBuilder From(string from)
 {
	 email.From = from;
	 return this;
 }
 // other fluent members here
}

public class MailService
{
 public class EmailBuilder { ... }
 private void SendEmailInternal(Email email) {}
 public void SendEmail(Action<EmailBuilder> builder)
 {
	 var email = new Email();
	 builder(new EmailBuilder(email));
	 SendEmailInternal(email);
 }
}

var ms = new MailService();
ms.SendEmail(email => email.From("foo@bar.com")
	.To("bar@baz.com")
	.Body("Hello, how are you?"));
```

##### Builder Extension with Recursive Generics
```cs
public abstract class PersonBuilder
{
 protected Person person = new Person();
 public Person Build()
 {
	 return person;
 }
}

public class PersonInfoBuilder<SELF> : PersonBuilder
 where SELF : PersonInfoBuilder<SELF>
{
 public SELF Called(string name)
 {
	 person.Name = name;
	 return (SELF) this;
 }
}

public class PersonJobBuilder<SELF> : PersonInfoBuilder<PersonJobBuilder<SELF>>
 where SELF : PersonJobBuilder<SELF>
{
 public SELF WorksAsA(string position)
 {
	 person.Position = position;
	 return (SELF) this;
 }
}

public class PersonBirthDateBuilder<SELF> : PersonJobBuilder<PersonBirthDateBuilder<SELF>>
 where SELF : PersonBirthDateBuilder<SELF>
{
 public SELF Born(DateTime dateOfBirth)
 {
	 person.DateOfBirth = dateOfBirth;
	 return (SELF)this;
 }
}

public class Person
{
 public class Builder : PersonJobBuilder<Builder>
 {
 internal Builder() {}
 }
 public static Builder New => new Builder();
 // other members omitted
}

var builder = Person.New
 .Called("Natasha")
 .WorksAsA("Doctor")
 .Born(new DateTime(1981, 1, 1));
````

##### Lazy Functional Builder
```cs
public sealed class PersonBuilder
{
	 private readonly List<Func<Person, Person>> actions = new ();
	 public PersonBuilder Do(Action<Person> action) => AddAction(action);
	 public Person Build() => actions.Aggregate(new Person(), (p, f) => f(p));
	 private PersonBuilder AddAction(Action<Person> action)
	 {
		 actions.Add(p => { action(p); return p; });
		 return this;
	 }

	 public PersonBuilder Called(string name) => Do(p => p.Name = name);
}

public static class PersonBuilderExtensions
{
 public static PersonBuilder WorksAs(this PersonBuilder builder, string position)
	 => builder.Do(p => p.Position = position);
}

var person = new PersonBuilder()
 .Called("Dmitri")
 .WorksAs("Programmer")
 .Build();
```
```cs
public abstract class FunctionalBuilder<TSubject, TSelf>
 where TSelf: FunctionalBuilder<TSubject, TSelf>
 where TSubject : new()

 private readonly List<Func<TSubject, TSubject>> actions = new();
 public TSelf Do(Action<TSubject> action) => AddAction(action);
 private TSelf AddAction(Action<TSubject> action)
 {
	 actions.Add(p => {
			 action(p);
			 return p; });
	 return (TSelf) this;
 }
 public TSubject Build() => actions.Aggregate(new TSubject(), (p, f) => f(p));
}

public sealed class PersonBuilder
 : FunctionalBuilder<Person, PersonBuilder>
{
 public PersonBuilder Called(string name) => Do(p => p.Name = name);
}
```

##### Scoping Builder Method
```cs
var cb = new CodeBuilder();
cb.AppendLine("class Foo")
	.Scope(b =>
	{
	    b.AppendLine("int bar;");
	});

class Foo
{
    // inside the scope
}

class CodeBuilder
{
	 public StringBuilder AppendLine()
	 {
		 builder.AppendLine();
		 return this;
	 }
	 public CodeBuilder Append(char value, int repeatCount)
	 {
		 builder.Append(value, repeatCount);
		 return this; // return a CodeBuilder, not a StringBuilder!
	 }
    public CodeBuilder Scope(Action<CodeBuilder> builder)
    {
        AppendLine("{");
        indentLevel++;
        builder(this);
        indentLevel--;
        return AppendLine("}");
    }
}
```

#### Factory
##### Factory Method
**Factory Method** is a creational design pattern that provides an interface for creating objects in a superclass, but allows subclasses to alter the type of objects that will be created.

- **Use the Factory Method when you don’t know beforehand the exact types and dependencies of the objects your code should work with.** The Factory Method separates product construction code from the code that actually uses the product. Therefore it’s easier to extend the product construction code independently from the rest of the code. For example, to add a new product type to the app, you’ll only need to create a new creator subclass and override the factory method in it.
- **Use the Factory Method when you want to provide users of your library or framework with a way to extend its internal components.** Inheritance is probably the easiest way to extend the default behavior of a library or framework. But how would the framework recognize that your subclass should be used instead of a standard component? The solution is to reduce the code that constructs components across the framework into a single factory method and let anyone override this method in addition to extending the component itself.
- **Use the Factory Method when you want to save system resources by reusing existing objects instead of rebuilding them each time.** You often experience this need when dealing with large, resource-intensive objects such as database connections, file systems, and network resources. Therefore, you need to have a regular method capable of creating new objects as well as reusing existing ones. That sounds very much like a factory method.

```cs
// The Creator class declares the factory method that is supposed to return
// an object of a Product class. The Creator's subclasses usually provide
// the implementation of this method.
abstract class Creator
{
    // Note that the Creator may also provide some default implementation of
    // the factory method.
    public abstract IProduct FactoryMethod();

    // Also note that, despite its name, the Creator's primary
    // responsibility is not creating products. Usually, it contains some
    // core business logic that relies on Product objects, returned by the
    // factory method. Subclasses can indirectly change that business logic
    // by overriding the factory method and returning a different type of
    // product from it.
    public string SomeOperation()
    {
        // Call the factory method to create a Product object.
        var product = FactoryMethod();
        // Now, use the product.
        var result = "Creator: The same creator's code has just worked with "
            + product.Operation();

        return result;
    }
}

// Concrete Creators override the factory method in order to change the
// resulting product's type.
class ConcreteCreator1 : Creator
{
    // Note that the signature of the method still uses the abstract product
    // type, even though the concrete product is actually returned from the
    // method. This way the Creator can stay independent of concrete product
    // classes.
    public override IProduct FactoryMethod()
    {
        return new ConcreteProduct1();
    }
}

class ConcreteCreator2 : Creator
{
    public override IProduct FactoryMethod()
    {
        return new ConcreteProduct2();
    }
}

// The Product interface declares the operations that all concrete products
// must implement.
public interface IProduct
{
    string Operation();
}

// Concrete Products provide various implementations of the Product
// interface.
class ConcreteProduct1 : IProduct
{
    public string Operation()
    {
        return "{Result of ConcreteProduct1}";
    }
}

class ConcreteProduct2 : IProduct
{
    public string Operation()
    {
        return "{Result of ConcreteProduct2}";
    }
}
```

```cs
var point = Point.NewPolarPoint(5, Math.PI / 4);
var point = Point.Factory.NewCartesianPoint(2, 3);

public class Point
{
    protected Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public static Point NewPolarPoint(double rho, double theta)
    {
        return new Point(rho * Math.Cos(theta), rho * Math.Sin(theta));
    }

    public static class Factory
	  {
		  public static Point NewCartesianPoint(double x, double y)
		  {
			  return new Point(x, y); // using a private constructor
		  }
	  }
}
```

##### Asynchronous Factory Method
```cs
var foo = await Foo.CreateAsync();

public class Foo
{
	protected Foo() { /* init here */ }
	public static Task<Foo> CreateAsync()
	{
		var result = new Foo();
		return result.InitAsync();
	}
}
```

##### Delegate Factories in IOC
```cs
public class DomainObject
{
 public delegate DomainObject Factory(int value);
 // other members here
}

var factory = container.Resolve<DomainObject.Factory>();
var dobj2 = factory(42);
Console.WriteLine(dobj2); // I have 42
```

##### Abstract Factory
**Abstract Factory** is a creational design pattern that lets you produce families of related objects without specifying their concrete classes.

- **Use the Abstract Factory when your code needs to work with various families of related products, but you don’t want it to depend on the concrete classes of those products—they might be unknown beforehand or you simply want to allow for future extensibility.** The Abstract Factory provides you with an interface for creating objects from each class of the product family. As long as your code creates objects via this interface, you don’t have to worry about creating the wrong variant of a product which doesn’t match the products already created by your app.
- **Consider implementing the Abstract Factory when you have a class with a set of Factory Methods that blur its primary responsibility.** In a well-designed program *each class is responsible only for one thing*. When a class deals with multiple product types, it may be worth extracting its factory methods into a stand-alone factory class or a full-blown Abstract Factory implementation.

```cs
var basic = GetFactory(false);
var basicRectangle = basic.Create(Shape.Rectangle);
var roundedSquare = GetFactory(true).Create(Shape.Square);

static ShapeFactory GetFactory(bool rounded)
{
    if (rounded)
        return new RoundedShapeFactory();
    else
        return new BasicShapeFactory();
}

public enum Shape
{
    Square,
    Rectangle
}

public abstract class ShapeFactory
{
    public abstract IShape Create(Shape shape);
}

public class BasicShapeFactory : ShapeFactory
{
    public override IShape Create(Shape shape)
    {
        switch (shape)
        {
            case Shape.Square:
                return new Square();
            case Shape.Rectangle:
                return new Rectangle();
            default:
                throw new ArgumentOutOfRangeException(
                nameof(shape), shape, null);
        }
    }
}
```
##### Object Tracking and Bulk Replacements
In real-world terms, you can think of this functionality as a factory recall: if a car factory finds that cars sold up to now have a defective part, it collects your car and replaces the part

```cs
var factory = new TrackingThemeFactory();
var theme = factory.CreateTheme(true);
var theme2 = factory.CreateTheme(false);
Console.WriteLine(factory.Info);

public interface ITheme
{
    string TextColor { get; }
    string BgrColor { get; }
}
class LightTheme : ITheme
{
    public string TextColor => "black";
    public string BgrColor => "white";
}
class DarkTheme : ITheme
{
    public string TextColor => "white";
    public string BgrColor => "dark gray";
}

public class TrackingThemeFactory
{
    private readonly List<WeakReference<ITheme>> themes = new();
    public ITheme CreateTheme(bool dark)
    {
        ITheme theme = dark ? new DarkTheme() : new LightTheme();
        themes.Add(new WeakReference<ITheme>(theme));
        return theme;
    }

    public string Info
    {
        get
        {
            // pretty-print what's in themes
        }
    }
}
```

```cs
var factory2 = new ReplaceableThemeFactory();
var magicTheme = factory2.CreateTheme(true);
Console.WriteLine(magicTheme.BgrColor); // dark gray
factory2.ReplaceTheme(false);
Console.WriteLine(magicTheme.BgrColor); // white

public class Ref<T> where T : class
{
    public T Value;
    public Ref(T value) { Value = value; }
}

public class ThemeRef : Ref<ITheme>, ITheme
{
    public ThemeRef(ITheme value) : base(value) { }
    public string TextColor => Value.TextColor;
    public string BgrColor => Value.BgrColor;
}

public class ReplaceableThemeFactory
{
    private readonly List<WeakReference<Ref<ITheme>>> themes = new();
    private ITheme createThemeImpl(bool dark)
    {
        return dark ? new DarkTheme() : new LightTheme();
    }

    public ITheme CreateTheme(bool dark)
    {
        var r = new ThemeRef(createThemeImpl(dark));
        themes.Add(new(r));
        return r;
    }

    public void ReplaceTheme(bool dark)
    {
        foreach (var wr in themes)
        {
            if (wr.TryGetTarget(out var reference))
            {
                reference.Value = createThemeImpl(dark);
            }
        }
    }
}
```

#### Prototype
**Prototype** is a creational design pattern that lets you copy existing objects without making your code dependent on their classes.

- **Use the Prototype pattern when your code shouldn’t depend on the concrete classes of objects that you need to copy.** This happens a lot when your code works with objects passed to you from 3rd-party code via some interface. The concrete classes of these objects are unknown, and you couldn’t depend on them even if you wanted to. The Prototype pattern provides the client code with a general interface for working with all objects that support cloning. This interface makes the client code independent from the concrete classes of objects that it clones.
- **Use the pattern when you want to reduce the number of subclasses that only differ in the way they initialize their respective objects.** Suppose you have a complex class that requires a laborious configuration before it can be used. There are several common ways to configure this class, and this code is scattered through your app. To reduce the duplication, you create several subclasses and put every common configuration code into their constructors. You solved the duplication problem, but now you have lots of dummy subclasses. The Prototype pattern lets you use a set of pre-built objects configured in various ways as prototypes. Instead of instantiating a subclass that matches some configuration, the client can simply look for an appropriate prototype and clone it.

##### Deep Copying with Interface
```cs
public interface IDeepCopyable<T> where T : new()
{
 void CopyTo(T target);
 public T DeepCopy()
 {
	 T t = new T();
	 CopyTo(t);
	 return t;
 }
}

public class Person : IDeepCopyable<Person>
{
 public string[] Names;
 public Address Address;
 public void CopyTo(Person target)
 {
	 target.Names = (string[]) Names.Clone();
	 target.Address = Address.DeepCopy(); // <-- extension method call
 }
}

public class Employee : Person, IDeepCopyable<Employee>
{
 public int Salary;
 public void CopyTo(Employee target)
 {
	 base.CopyTo(target); // <-- extension method call on base class
	 target.Salary = Salary;
 }
}

//
public static T DeepCopy<T>(this IDeepCopyable<T> item)
 where T : new()
{
 return item.DeepCopy();
}

var john = new Employee();
Person p = john.DeepCopy<Person>();
Employee e = john.DeepCopy<Employee>();

//
public static T DeepCopy<T>(this T person)
 where T : Person, new()
{
 return ((IDeepCopyable<T>) person).DeepCopy();
}

var john = new Employee();
john.Names = new[] {"John", "Doe"};
john.Address = new Address {
 HouseNumber = 123, StreetName = "London Road"
};
john.Salary = 321000;
var copy = john.DeepCopy(); // of type Employee
```
##### Guidelines
```cs
public Person DeepCopy()
{
 var copy = new Person();
 copy.Names = (string[]) Names.Clone();
 copy.Addresses = Array.ConvertAll(Addresses, a => a.DeepCopy());
 return copy;
}

// Common Collection Types
List<int> items = new(){1, 2, 3};
List<int> replica = new(items); // copy constructor

var people = new Dictionary<string, Address>
{
 ["John"] = new(38, "London Road"),
 ["Jane"] = new(72, "Jane Street")
};
var peopleCopies = people.ToDictionary(
 x => x.Key,
 x => x.Value.DeepCopy());
```

##### Prototype Factory

```cs
private static Person NewEmployee(Person proto, string name, int suite)
 {
	 var copy = proto.DeepCopy();
	 copy.Name = name;
	 copy.Address.Suite = suite;
	 return copy;
 }
```

#### Singleton
**Singleton** is a creational design pattern that lets you ensure that a class has only one instance, while providing a global access point to this instance.

- **Use the Singleton pattern when a class in your program should have just a single instance available to all clients; for example, a single database object shared by different parts of the program.** The Singleton pattern disables all other means of creating objects of a class except for the special creation method. This method either creates a new object or returns an existing one if it has already been created.
- **Use the Singleton pattern when you need stricter control over global variables.** Unlike global variables, the Singleton pattern guarantees that there’s just one instance of a class. Nothing, except for the Singleton class itself, can replace the cached instance. Note that you can always adjust this limitation and allow creating any number of Singleton instances. The only piece of code that needs changing is the body of the `Instance` method.

#####

```cs Classic Implementation
 public class MyDatabase
 {
    private MyDatabase()
    {
        Console.WriteLine("Initializing  database");
    }

    private static Lazy<MyDatabase> instance = new();

    public static MyDatabase Instance => instance.Value;
}

public abstract class Singleton<T> where T : Singleton<T>
{
    private static readonly Lazy<T> Lazy
	    = new(() => Activator.CreateInstance(typeof(T), nonPublic: true) as T);
    public static T Instance => Lazy.Value;
}

class MyDatabase : Singleton<MyDatabase>
{
    private MyDatabase() { /* load data here */ }
}
```

##### The Trouble with Singleton
```cs
public interface IDatabase
{
    int GetPopulation(string name);
}

public class SingletonDatabase : IDatabase
{
    private Dictionary<string, int> capitals;

    private SingletonDatabase()
    {
        WriteLine("Initializing database");
        capitals = File.ReadAllLines(
        Path.Combine(
        new FileInfo(typeof(IDatabase).Assembly.Location).DirectoryName,
        "capitals.txt")
        )
        .Batch(2) // from MoreLINQ
        .ToDictionary(
        list => list.ElementAt(0).Trim(),
        list => int.Parse(list.ElementAt(1)));
    }

    public int GetPopulation(string name)
    {
        return capitals[name];
    }

    private static Lazy<SingletonDatabase> instance =
         new Lazy<SingletonDatabase>(() =>
         {
             return new SingletonDatabase();
         });

    public static IDatabase Instance => instance.Value;
}

public class ConfigurableRecordFinder
{
	private IDatabase database;
	public ConfigurableRecordFinder(IDatabase database)
	{
		 this.database = database;
	}
  public int GetTotalPopulation(IEnumerable<string> names)
  {
		int result = 0;
		foreach (var name in names)
		 result += database.GetPopulation(name);
		return result;
  }
}
```

##### Per-Thread Singleton
```cs
public sealed class PerThreadSingleton
{
    private static ThreadLocal<PerThreadSingleton> threadInstance
        = new(() => new PerThreadSingleton());
    public int Id;
    private PerThreadSingleton()
    {
        Id = Thread.CurrentThread.ManagedThreadId;
    }
    public static PerThreadSingleton Instance => threadInstance.Value;
}

var t1 = Task.Factory.StartNew(() =>
{
	Console.WriteLine("t1: " + PerThreadSingleton.Instance.Id);
});

var t2 = Task.Factory.StartNew(() =>
{
	Console.WriteLine("t2: " + PerThreadSingleton.Instance.Id);
	Console.WriteLine("t2 again: " + PerThreadSingleton.Instance.Id);
});

Task.WaitAll(t1, t2);
```

##### Monostate
It is a class that behaves like a singleton while appearing as an ordinary class. The class appears as an ordinary class with getters and setters, but they actually work on static data.

```cs
 public class ChiefExecutiveOfficer
 {
    private static string name;
    private static int age;
    public string Name
    {
        get => name;
        set => name = value;
    }
    public int Age
    {
        get => age;
        set => age = value;
    }
 }
```

##### Multiton
instead of forcing us to have just one instance, gets us to have a finite number of named instances of some particular component.

```cs
enum Subsystem
{
    Main,
    Backup
}

class Printer
{
    private Printer() { }
    public static Printer Get(Subsystem ss)
    {
        if (instances.TryGetValue(ss, out var value))
        {
            return value;
        }
        value = new Printer();
        return value;
    }
    private static ConcurrentDictionary<Subsystem, Printer> instances = new();
}
```

#### Adapter
**Adapter** is a structural design pattern that allows objects with incompatible interfaces to collaborate.

- **Use the Adapter class when you want to use some existing class, but its interface isn’t compatible with the rest of your code.** The Adapter pattern lets you create a middle-layer class that serves as a translator between your code and a legacy class, a 3rd-party class or any other class with a weird interface.
- **Use the pattern when you want to reuse several existing subclasses that lack some common functionality that can’t be added to the superclass.** You could extend each subclass and put the missing functionality into new child classes. However, you’ll need to duplicate the code across all of these new classes, which smells really bad. The much more elegant solution would be to put the missing functionality into an adapter class. Then you would wrap objects with missing features inside the adapter, gaining needed features dynamically. For this to work, the target classes must have a common interface, and the adapter’s field should follow that interface. This approach looks very similar to the Decorator pattern.

##### Generic Adapter Value

```cs
var v = new Vector<int, Dimensions.Two>();
var v2 = new Vector2i();
v2.Y = 456;
public class Vector2i : Vector<int, Dimensions.Two>
{
    public Vector2i() { }
    public Vector2i(params int[] values) : base(values) { }

    public int X
    {
        get => data[0];
        set => data[0] = value;
    }
    // similarly for Y
    public int Y
    {
        get => data[1];
        set => data[1] = value;
    }
}

public class Vector3i : VectorOfInt<Dimensions.Three>
{
    public Vector3i(params int[] values) : base(values)
    {
    }
}

public interface IInteger
{
    int Value { get; }
}

public static class Dimensions
{
    public readonly struct Two : IInteger
    {
        public int Value => 2;
    }
    public readonly struct Three : IInteger
    {
        public int Value => 3;
    }
}

public class Vector<T, D> where D : IInteger, new()
{
    protected T[] data;
    public Vector()
    {
        data = new T[new D().Value];
    }

    public Vector(params T[] values)
    {
        var requiredSize = new D().Value;
        data = new T[requiredSize];
        var providedSize = values.Length;
        for (int i = 0; i < Math.Min(requiredSize, providedSize); ++i)
            data[i] = values[i];
    }

    public T this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }
}

public class VectorOfInt<D> : Vector<int, D> where D : IInteger, new()
{
    public VectorOfInt() { }
    public VectorOfInt(params int[] values) : base(values) { }
    public static VectorOfInt<D> operator +(VectorOfInt<D> lhs, VectorOfInt<D> rhs)
    {
        var result = new VectorOfInt<D>();
        var dim = new D().Value;
        for (int i = 0; i < dim; i++)
        {
            result[i] = lhs[i] + rhs[i];
        }
        return result;
    }
}
```

##### Conceptual example

```cs
public interface ITarget
{
    string GetRequest();
}

// The Adaptee contains some useful behavior, but its interface is incompatible with
// the existing client code. The Adaptee needs some adaptation before the client code can use it.
class Adaptee
{
    public string GetSpecificRequest()
    {
        return "Specific request.";
    }
}

// The Adapter makes the Adaptee's interface compatible with the Target's interface.
class Adapter : ITarget
{
    private readonly Adaptee _adaptee;

    public Adapter(Adaptee adaptee)
    {
        this._adaptee = adaptee;
    }

    public string GetRequest()
    {
        return $"This is '{this._adaptee.GetSpecificRequest()}'";
    }
}
```

#### Bridge
**Bridge** is a structural design pattern that lets you split a large class or a set of closely related classes into two separate hierarchies - abstraction and implementation - which can be developed independently of each other.

- **Use the Bridge pattern when you want to divide and organize a monolithic class that has several variants of some functionality (for example, if the class can work with various database servers).** The bigger a class becomes, the harder it is to figure out how it works, and the longer it takes to make a change. The changes made to one of the variations of functionality may require making changes across the whole class, which often results in making errors or not addressing some critical side effects.The Bridge pattern lets you split the monolithic class into several class hierarchies. After this, you can change the classes in each hierarchy independently of the classes in the others. This approach simplifies code maintenance and minimizes the risk of breaking existing code.
- **Use the pattern when you need to extend a class in several orthogonal (independent) dimensions.** The Bridge suggests that you extract a separate class hierarchy for each of the dimensions. The original class delegates the related work to the objects belonging to those hierarchies instead of doing everything on its own.
- **Use the Bridge if you need to be able to switch implementations at runtime.** Although it’s optional, the Bridge pattern lets you replace the implementation object inside the abstraction. It’s as easy as assigning a new value to a field. By the way, this last item is the main reason why so many people confuse the Bridge with the Strategy pattern. Remember that a pattern is more than just a certain way to structure your classes. It may also communicate intent and a problem being addressed.

##### Conventional Bridge
```cs
var raster = new RasterRenderer();
var vector = new VectorRenderer();
var circle = new Circle(vector, 5);
circle.Draw(); // Drawing a circle of radius 5
circle.Resize(2);
circle.Draw(); // Drawing a circle of radius 10

public interface IRenderer
{
    void RenderCircle(float radius);
    // RenderSquare, RenderTriangle, etc.
}

public abstract class Shape
{
    protected IRenderer renderer;
    // a bridge between the shape that's being drawn an
    // the component which actually draws it
    public Shape(IRenderer renderer)
    {
        this.renderer = renderer;
    }
    public abstract void Draw();
    public abstract void Resize(float factor);
}

public class Circle : Shape
{
    private float radius;
    public Circle(IRenderer renderer, float radius) : base(renderer)
    {
        this.radius = radius;
    }
    public override void Draw()
    {
        renderer.RenderCircle(radius);
    }
    public override void Resize(float factor)
    {
        radius *= factor;
    }
}

public class VectorRenderer : IRenderer
{
    public void RenderCircle(float radius)
    {
        WriteLine($"Drawing a circle of radius {radius}");
    }
}

public class RasterRenderer : IRenderer
{
    public void RenderCircle(float radius)
    {
        WriteLine($"Drawing a circle of radius {radius} using raster graphics");
    }
}
```

##### Conceptual Example

```cs
class Abstraction
{
    protected IImplementation _implementation;

    public Abstraction(IImplementation implementation)
    {
        this._implementation = implementation;
    }

    public virtual string Operation()
    {
        return "Abstract: Base operation with:\n" +
            _implementation.OperationImplementation();
    }
}

class ExtendedAbstraction : Abstraction
{
    public ExtendedAbstraction(IImplementation implementation) : base(implementation)
    {
    }

    public override string Operation()
    {
        return "ExtendedAbstraction: Extended operation with:\n" +
            base._implementation.OperationImplementation();
    }
}

// The Implementation defines the interface for all implementation classes.
// It doesn't have to match the Abstraction's interface. In fact, the two
// interfaces can be entirely different. Typically the Implementation
// interface provides only primitive operations, while the Abstraction
// defines higher- level operations based on those primitives.
public interface IImplementation
{
    string OperationImplementation();
}

// Each Concrete Implementation corresponds to a specific platform and
// implements the Implementation interface using that platform's API.
class ConcreteImplementationA : IImplementation
{
    public string OperationImplementation()
    {
        return "ConcreteImplementationA: The result in platform A.\n";
    }
}

class ConcreteImplementationB : IImplementation
{
    public string OperationImplementation()
    {
        return "ConcreteImplementationB: The result in platform B.\n";
    }
}

Abstraction abstraction;

abstraction = new Abstraction(new ConcreteImplementationA());
abstraction.Operation()

abstraction = new ExtendedAbstraction(new ConcreteImplementationB());
abstraction.Operation()
```

#### Composite
**Composite** is a structural design pattern that lets you compose objects into tree structures and then work with these structures as if they were individual objects.

- **Use the Composite pattern when you have to implement a tree-like object structure.** The Composite pattern provides you with two basic element types that share a common interface: simple leaves and complex containers. A container can be composed of both leaves and other containers. This lets you construct a nested recursive object structure that resembles a tree.
- **Use the pattern when you want the client code to treat both simple and complex elements uniformly.** All elements defined by the Composite pattern share a common interface. Using this interface, the client doesn’t have to worry about the concrete class of the objects it works with.

##### Conceptual Example
```cs
// The base Component class declares common operations for both simple and
// complex objects of a composition.
abstract class Component
{
    public Component() { }

    // The base Component may implement some default behavior or leave it to
    // concrete classes (by declaring the method containing the behavior as
    // "abstract").
    public abstract string Operation();

    // In some cases, it would be beneficial to define the child-management
    // operations right in the base Component class. This way, you won't
    // need to expose any concrete component classes to the client code,
    // even during the object tree assembly. The downside is that these
    // methods will be empty for the leaf-level components.
    public virtual void Add(Component component)
    {
        throw new NotImplementedException();
    }

    public virtual void Remove(Component component)
    {
        throw new NotImplementedException();
    }

    // You can provide a method that lets the client code figure out whether
    // a component can bear children.
    public virtual bool IsComposite()
    {
        return true;
    }
}

// The Leaf class represents the end objects of a composition. A leaf can't
// have any children.
//
// Usually, it's the Leaf objects that do the actual work, whereas Composite
// objects only delegate to their sub-components.
class Leaf : Component
{
    public override string Operation()
    {
        return "Leaf";
    }

    public override bool IsComposite()
    {
        return false;
    }
}

// The Composite class represents the complex components that may have
// children. Usually, the Composite objects delegate the actual work to
// their children and then "sum-up" the result.
class Composite : Component
{
    protected List<Component> _children = new List<Component>();

    public override void Add(Component component)
    {
        this._children.Add(component);
    }

    public override void Remove(Component component)
    {
        this._children.Remove(component);
    }

    // The Composite executes its primary logic in a particular way. It
    // traverses recursively through all its children, collecting and
    // summing their results. Since the composite's children pass these
    // calls to their children and so forth, the whole object tree is
    // traversed as a result.
    public override string Operation()
    {
        int i = 0;
        string result = "Branch(";

        foreach (Component component in this._children)
        {
            result += component.Operation();
            if (i != this._children.Count - 1)
            {
                result += "+";
            }
            i++;
        }

        return result + ")";
    }
}

class Client
{
    // The client code works with all of the components via the base
    // interface.
    public void ClientCode(Component leaf)
    {
        Console.WriteLine($"RESULT: {leaf.Operation()}\n");
    }

    // Thanks to the fact that the child-management operations are declared
    // in the base Component class, the client code can work with any
    // component, simple or complex, without depending on their concrete
    // classes.
    public void ClientCode2(Component component1, Component component2)
    {
        if (component1.IsComposite())
        {
            component1.Add(component2);
        }

        Console.WriteLine($"RESULT: {component1.Operation()}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Client client = new Client();

        // This way the client code can support the simple leaf
        // components...
        Leaf leaf = new Leaf();
        Console.WriteLine("Client: I get a simple component:");
        client.ClientCode(leaf);

        // ...as well as the complex composites.
        Composite tree = new Composite();
        Composite branch1 = new Composite();
        branch1.Add(new Leaf());
        branch1.Add(new Leaf());
        Composite branch2 = new Composite();
        branch2.Add(new Leaf());
        tree.Add(branch1);
        tree.Add(branch2);
        Console.WriteLine("Client: Now I've got a composite tree:");
        client.ClientCode(tree);

        Console.Write("Client: I don't need to check the components classes even when managing the tree:\n");
        client.ClientCode2(tree, leaf);
    }
}
```

##### Grouping Graphic Objects
```cs
public class GraphicObject
 {
	 private readonly Lazy<List<GraphicObject>> children = new();
	 public List<GraphicObject> Children => children.Value;

	 private void Print(StringBuilder sb, int depth)
	 {
		 sb.Append(new string('*', depth))
			 .Append(string.IsNullOrWhiteSpace(Color) ? string.Empty : $"{Color} ")
			 .AppendLine($"{Name}");
		 foreach (var child in Children)
			 child.Print(sb, depth + 1);
	 }

	 public override string ToString()
	 {
		 var sb = new StringBuilder();
		 Print(sb, 0);
		 return sb.ToString();
	 }
 }

 public class Circle : GraphicObject
 {
	 public override string Name => "Circle";
 }
 public class Square : GraphicObject
 {
	 public override string Name => "Square";
 }

var drawing = new GraphicObject {Name = "My Drawing"};
drawing.Children.Add(new Square {Color = "Red"});
drawing.Children.Add(new Circle{Color="Yellow"});
var group = new GraphicObject();
group.Children.Add(new Circle{Color="Blue"});
group.Children.Add(new Square{Color="Blue"});
drawing.Children.Add(group);
```

##### Neural Networks
```cs
public class Neuron : IEnumerable<Neuron>
{
 public List<Neuron> In, Out;

 public IEnumerator<Neuron> GetEnumerator()
 {
	 yield return this;
 }

 IEnumerator IEnumerable.GetEnumerator()
 {
	 return GetEnumerator();
 }
}


public static class ExtensionMethods
{
 public static void ConnectTo(this IEnumerable<Neuron> self, IEnumerable<Neuron> other)
 {
	 if (ReferenceEquals(self, other)) return;
	 foreach (var from in self)
		 foreach (var to in other)
		 {
			 from.Out.Add(to);
			 to.In.Add(from);
		 }
	 }
}

```

##### Shrink Wrapping the Composite
```cs
public abstract class Scalar<T> : IEnumerable<T> where T : Scalar<T>
{
 public IEnumerator<T> GetEnumerator()
 {
	 yield return (T) this;
 }
 IEnumerator IEnumerable.GetEnumerator()
 {
	 return GetEnumerator();
 }
}
```

##### Composite Specification
```cs
 public abstract class CompositeSpecification<T> : ISpecification<T>
 {
	 protected readonly ISpecification<T>[] items;
	 public CompositeSpecification(params ISpecification<T>[] items)
	 {
		 this.items = items;
	 }
 }

public class AndSpecification<T> : CompositeSpecification<T>
{
	public AndSpecification(params ISpecification<T>[] items) :
		base(items) {}
	public override bool IsSatisfied(T t)
	{
		return items.All(i => i.IsSatisfied(t));
	}
}
```

#### Decorator
**Decorator** is a structural design pattern that lets you attach new behaviors to objects by placing these objects inside special wrapper objects that contain the behaviors.

- **Use the Decorator pattern when you need to be able to assign extra behaviors to objects at runtime without breaking the code that uses these objects.** The Decorator lets you structure your business logic into layers, create a decorator for each layer and compose objects with various combinations of this logic at runtime. The client code can treat all these objects in the same way, since they all follow a common interface.
- **Use the pattern when it’s awkward or not possible to extend an object’s behavior using inheritance.** Many programming languages have the `final` keyword that can be used to prevent further extension of a class. For a final class, the only way to reuse the existing behavior would be to wrap the class with your own wrapper, using the Decorator pattern.

##### Conceptual example
```cs
// The base Component interface defines operations that can be altered by
// decorators.
public abstract class Component
{
    public abstract string Operation();
}

// Concrete Components provide default implementations of the operations.
// There might be several variations of these classes.
class ConcreteComponent : Component
{
    public override string Operation()
    {
        return "ConcreteComponent";
    }
}

// The base Decorator class follows the same interface as the other
// components. The primary purpose of this class is to define the wrapping
// interface for all concrete decorators. The default implementation of the
// wrapping code might include a field for storing a wrapped component and
// the means to initialize it.
abstract class Decorator : Component
{
    protected Component _component;

    public Decorator(Component component)
    {
        this._component = component;
    }

    public void SetComponent(Component component)
    {
        this._component = component;
    }

    // The Decorator delegates all work to the wrapped component.
    public override string Operation()
    {
        if (this._component != null)
        {
            return this._component.Operation();
        }
        else
        {
            return string.Empty;
        }
    }
}

// Concrete Decorators call the wrapped object and alter its result in some
// way.
class ConcreteDecoratorA : Decorator
{
    public ConcreteDecoratorA(Component comp) : base(comp)
    {
    }

    // Decorators may call parent implementation of the operation, instead
    // of calling the wrapped object directly. This approach simplifies
    // extension of decorator classes.
    public override string Operation()
    {
        return $"ConcreteDecoratorA({base.Operation()})";
    }
}

// Decorators can execute their behavior either before or after the call to
// a wrapped object.
class ConcreteDecoratorB : Decorator
{
    public ConcreteDecoratorB(Component comp) : base(comp)
    {
    }

    public override string Operation()
    {
        return $"ConcreteDecoratorB({base.Operation()})";
    }
}

var simple = new ConcreteComponent();
ConcreteDecoratorA decorator1 = new ConcreteDecoratorA(simple);
ConcreteDecoratorB decorator2 = new ConcreteDecoratorB(decorator1);
```

##### Multiple Inheritance with Interfaces and Default Interface Members
```cs
public interface ICreature
{
 int Age { get; set; }
}

public interface IBird : ICreature
{
 void Fly()
 {
	 if (Age >= 10)
		 WriteLine("I am flying");
 }
}

public interface ILizard : ICreature
{
 void Crawl()
 {
	 if (Age < 10)
		 WriteLine("I am crawling!");
 }
}

public class Dragon : IBird, ILizard
{
 public int Age { get; set; }
}

var d = new Dragon {Age = 5};
if (d is IBird bird)
 bird.Fly();
if (d is ILizard lizard)
 lizard.Crawl();
```

##### Dynamic Decorator Composition and  Decorator Cycle Policies and Static Decorator Composition
```cs
public abstract class ShapeDecoratorCyclePolicy
{
	public abstract bool TypeAdditionAllowed(Type type, IList<Type> allTypes);
	public abstract bool ApplicationAllowed(Type type, IList<Type> allTypes);
}


public class CyclesAllowedPolicy : ShapeDecoratorCyclePolicy
	{
	public override bool TypeAdditionAllowed(Type type, IList<Type> allTypes) => true;
	public override bool ApplicationAllowed(Type type, IList<Type> allTypes) => true;
}

public class ThrowOnCyclePolicy : ShapeDecoratorCyclePolicy
{
	private bool handler(Type type, IList<Type> allTypes)
	{
		 if (allTypes.Contains(type))
			 throw new InvalidOperationException($"Cycle detected! Type is already a {type.FullName}!");
		 return true;
	}
	public override bool TypeAdditionAllowed(Type type, IList<Type> allTypes)
	{
		 return handler(type, allTypes);
	}
	public override bool ApplicationAllowed(Type type, IList<Type> allTypes)
	{
		 return handler(type, allTypes);
	}
}

public class AbsorbCyclePolicy : ShapeDecoratorCyclePolicy
{
 public override bool TypeAdditionAllowed(Type type, IList<Type> allTypes)
 {
	 return true;
 }
 public override bool ApplicationAllowed(Type type, IList<Type> allTypes)
 {
	 return !allTypes.Contains(type);
 }
}

public abstract class ShapeDecorator : Shape
{
 protected internal readonly List<Type> types = new();
 protected internal Shape shape;
 public ShapeDecorator(Shape shape)
 {
	 this.shape = shape;
	 if (shape is ShapeDecorator sd)
		 types.AddRange(sd.types);
 }
}

public abstract class ShapeDecorator<TSelf, TCyclePolicy> : ShapeDecorator
 where TCyclePolicy : ShapeDecoratorCyclePolicy, new()
{
 private readonly TCyclePolicy policy = new();
 public ShapeDecorator(Shape shape) : base(shape)
 {
	 if (policy.TypeAdditionAllowed(typeof(TSelf), types))
		 types.Add(typeof(TSelf));
 }
}

public class ColoredShape : ShapeDecorator<ColoredShape, AbsorbCyclePolicy>
{
 private readonly string color;
 public ColoredShape(Shape shape, string color) : base(shape)
 {
	 this.color = color;
 }
 public override string AsString()
 {
	 var sb = new StringBuilder($"{shape.AsString()}");
	 if (policy.ApplicationAllowed(types[0], types.Skip(1).ToList()))
		 sb.Append($" has the color {color}");
	 return sb.ToString();
 }
}

 var circle = new Circle(2);
 WriteLine(circle.AsString());
 // A circle of radius 2
 var redSquare = new ColoredShape(circle, "red");
 WriteLine(redSquare.AsString());
 // A circle of radius 2 has the color red
 var redHalfTransparentSquare = new TransparentShape(redSquare, 0.5f);
 WriteLine(redHalfTransparentSquare.AsString());
 // A circle of radius 2 has the color red has 50% transparenc
```

#### Facade
**Facade** is a structural design pattern that provides a simplified interface to a library, a framework, or any other complex set of classes.

- **Use the Facade pattern when you need to have a limited but straightforward interface to a complex subsystem.** Often, subsystems get more complex over time. Even applying design patterns typically leads to creating more classes. A subsystem may become more flexible and easier to reuse in various contexts, but the amount of configuration and boilerplate code it demands from a client grows ever larger. The Facade attempts to fix this problem by providing a shortcut to the most-used features of the subsystem which fit most client requirements.
- **Use the Facade when you want to structure a subsystem into layers.** Create facades to define entry points to each level of a subsystem. You can reduce coupling between multiple subsystems by requiring them to communicate only through facades. For example, let’s return to our video conversion framework. It can be broken down into two layers: video- and audio-related. For each layer, you can create a facade and then make the classes of each layer communicate with each other via those facades. This approach looks very similar to the Mediator pattern.

##### Conceptual example
```cs
// The Facade class provides a simple interface to the complex logic of one
// or several subsystems. The Facade delegates the client requests to the
// appropriate objects within the subsystem. The Facade is also responsible
// for managing their lifecycle. All of this shields the client from the
// undesired complexity of the subsystem.
public class Facade
{
    protected Subsystem1 _subsystem1;

    protected Subsystem2 _subsystem2;

    public Facade(Subsystem1 subsystem1, Subsystem2 subsystem2)
    {
        this._subsystem1 = subsystem1;
        this._subsystem2 = subsystem2;
    }

    // The Facade's methods are convenient shortcuts to the sophisticated
    // functionality of the subsystems. However, clients get only to a
    // fraction of a subsystem's capabilities.
    public string Operation()
    {
        string result = "Facade initializes subsystems:\n";
        result += this._subsystem1.operation1();
        result += this._subsystem2.operation1();
        result += "Facade orders subsystems to perform the action:\n";
        result += this._subsystem1.operationN();
        result += this._subsystem2.operationZ();
        return result;
    }
}

// The Subsystem can accept requests either from the facade or client
// directly. In any case, to the Subsystem, the Facade is yet another
// client, and it's not a part of the Subsystem.
public class Subsystem1
{
    public string operation1()
    {
        return "Subsystem1: Ready!\n";
    }

    public string operationN()
    {
        return "Subsystem1: Go!\n";
    }
}

// Some facades can work with multiple subsystems at the same time.
public class Subsystem2
{
    public string operation1()
    {
        return "Subsystem2: Get ready!\n";
    }

    public string operationZ()
    {
        return "Subsystem2: Fire!\n";
    }
}

Subsystem1 subsystem1 = new Subsystem1();
Subsystem2 subsystem2 = new Subsystem2();
Facade facade = new Facade(subsystem1, subsystem2);
facade.Operation();
```

#### Flyweight
**Flyweight** is a structural design pattern that lets you fit more objects into the available amount of RAM by sharing common parts of state between multiple objects instead of keeping all of the data in each object.

- **Use the Flyweight pattern only when your program must support a huge number of objects which barely fit into available RAM.** The benefit of applying the pattern depends heavily on how and where it’s used. It’s most useful when:
    - an application needs to spawn a huge number of similar objects
    - this drains all available RAM on a target device
    - the objects contain duplicate states which can be extracted and shared between multiple objects

##### Conceptual Example
```cs
public class Flyweight
{
    private Car _sharedState;

    public Flyweight(Car car)
    {
        this._sharedState = car;
    }

    public void Operation(Car uniqueState)
    {
        string s = JsonConvert.SerializeObject(this._sharedState);
        string u = JsonConvert.SerializeObject(uniqueState);
        Console.WriteLine($"Flyweight: Displaying shared {s} and unique {u} state.");
    }
}

// The Flyweight Factory creates and manages the Flyweight objects. It
// ensures that flyweights are shared correctly. When the client requests a
// flyweight, the factory either returns an existing instance or creates a
// new one, if it doesn't exist yet.
public class FlyweightFactory
{
    private List<Tuple<Flyweight, string>> flyweights = new List<Tuple<Flyweight, string>>();

    public FlyweightFactory(params Car[] args)
    {
        foreach (var elem in args)
        {
            flyweights.Add(new Tuple<Flyweight, string>(new Flyweight(elem), this.getKey(elem)));
        }
    }

    // Returns a Flyweight's string hash for a given state.
    public string getKey(Car key)
    {
        List<string> elements = [key.Model, key.Color, key.Company];

        if (key.Owner != null && key.Number != null)
        {
            elements.Add(key.Number);
            elements.Add(key.Owner);
        }

        elements.Sort();

        return string.Join("_", elements);
    }

    // Returns an existing Flyweight with a given state or creates a new
    // one.
    public Flyweight GetFlyweight(Car sharedState)
    {
        string key = this.getKey(sharedState);

        var flyWeight = this.flyweights.FirstOrDefault(t => t.Item2 == key);

        if (flyWeight is null)
        {
            Console.WriteLine("FlyweightFactory: Can't find a flyweight, creating new one.");
            flyWeight = new Tuple<Flyweight, string>(new Flyweight(sharedState), key);
            this.flyweights.Add(flyWeight);
        }
        else
        {
            Console.WriteLine("FlyweightFactory: Reusing existing flyweight.");
        }

        return flyWeight.Item1;
    }

    public void listFlyweights()
    {
        var count = flyweights.Count;
        Console.WriteLine($"\nFlyweightFactory: I have {count} flyweights:");
        foreach (var flyweight in flyweights)
        {
            Console.WriteLine(flyweight.Item2);
        }
    }
}

public class Car
{
    public string Owner { get; set; }

    public string Number { get; set; }

    public string Company { get; set; }

    public string Model { get; set; }

    public string Color { get; set; }
}

static void addCarToPoliceDatabase(FlyweightFactory factory, Car car)
{
    Console.WriteLine("\nClient: Adding a car to database.");

    var flyweight = factory.GetFlyweight(new Car
    {
        Color = car.Color,
        Model = car.Model,
        Company = car.Company
    });

    // The client code either stores or calculates extrinsic state and
    // passes it to the flyweight's methods.
    flyweight.Operation(car);
}

var factory = new FlyweightFactory(
    new Car { Company = "Chevrolet", Model = "Camaro2018", Color = "pink" },
    new Car { Company = "Mercedes Benz", Model = "C300", Color = "black" },
    new Car { Company = "Mercedes Benz", Model = "C500", Color = "red" },
    new Car { Company = "BMW", Model = "M5", Color = "red" },
    new Car { Company = "BMW", Model = "X6", Color = "white" }
);
factory.listFlyweights();

addCarToPoliceDatabase(factory, new Car
{
    Number = "CL234IR",
    Owner = "James Doe",
    Company = "BMW",
    Model = "M5",
    Color = "red"
});

addCarToPoliceDatabase(factory, new Car
{
    Number = "CL234IR",
    Owner = "James Doe",
    Company = "BMW",
    Model = "X1",
    Color = "red"
});

factory.listFlyweights();
```

#### Proxy
**Proxy** is a structural design pattern that lets you provide a substitute or placeholder for another object. A proxy controls access to the original object, allowing you to perform something either before or after the request gets through to the original object.

- **Lazy initialization (virtual proxy). This is when you have a heavyweight service object that wastes system resources by being always up, even though you only need it from time to time.** Instead of creating the object when the app launches, you can delay the object’s initialization to a time when it’s really needed.
- **Access control (protection proxy). This is when you want only specific clients to be able to use the service object; for instance, when your objects are crucial parts of an operating system and clients are various launched applications (including malicious ones).** The proxy can pass the request to the service object only if the client’s credentials match some criteria.
- **Local execution of a remote service (remote proxy). This is when the service object is located on a remote server.** In this case, the proxy passes the client request over the network, handling all of the nasty details of working with the network.
- **Logging requests (logging proxy). This is when you want to keep a history of requests to the service object.** The proxy can log each request before passing it to the service.
- **Caching request results (caching proxy). This is when you need to cache results of client requests and manage the life cycle of this cache, especially if results are quite large.** The proxy can implement caching for recurring requests that always yield the same results. The proxy may use the parameters of requests as the cache keys.
- **Smart reference. This is when you need to be able to dismiss a heavyweight object once there are no clients that use it.** The proxy can keep track of clients that obtained a reference to the service object or its results. From time to time, the proxy may go over the clients and check whether they are still active. If the client list gets empty, the proxy might dismiss the service object and free the underlying system resources. The proxy can also track whether the client had modified the service object. Then the unchanged objects may be reused by other clients.

##### Conceptual example
```cs
// The Subject interface declares common operations for both RealSubject and
// the Proxy. As long as the client works with RealSubject using this
// interface, you'll be able to pass it a proxy instead of a real subject.
public interface ISubject
{
    void Request();
}

// The RealSubject contains some core business logic. Usually, RealSubjects
// are capable of doing some useful work which may also be very slow or
// sensitive - e.g. correcting input data. A Proxy can solve these issues
// without any changes to the RealSubject's code.
class RealSubject : ISubject
{
    public void Request()
    {
        Console.WriteLine("RealSubject: Handling Request.");
    }
}

// The Proxy has an interface identical to the RealSubject.
class Proxy : ISubject
{
    private RealSubject _realSubject;

    public Proxy(RealSubject realSubject)
    {
        this._realSubject = realSubject;
    }

    // The most common applications of the Proxy pattern are lazy loading,
    // caching, controlling the access, logging, etc. A Proxy can perform
    // one of these things and then, depending on the result, pass the
    // execution to the same method in a linked RealSubject object.
    public void Request()
    {
        if (this.CheckAccess())
        {
            this._realSubject.Request();

            this.LogAccess();
        }
    }

    public bool CheckAccess()
    {
        // Some real checks should go here.
        Console.WriteLine("Proxy: Checking access prior to firing a real request.");

        return true;
    }

    public void LogAccess()
    {
        Console.WriteLine("Proxy: Logging the time of request.");
    }
}


Console.WriteLine("Client: Executing the client code with a real subject:");
RealSubject realSubject = new RealSubject();
realSubject.Request();

Console.WriteLine("Client: Executing the same client code with a proxy:");
Proxy proxy = new Proxy(realSubject);
proxy.Request();
```

##### Composite Proxy: SoA/AoS (Array of Structures/Structure of Arrays)
```cs
public class Creatures
{

    public readonly int size;
    public byte[] age;
    public int[] x, y;
    public Creatures(int size)
    {
        this.size = size;
        age = new byte[size];
        x = new int[size];
        y = new int[size];
    }

    public Creature this[int index]
        => new Creature(this, index);
    public IEnumerator<Creature> GetEnumerator()
    {
        for (int pos = 0; pos < size; ++pos)
            yield return new Creature(this, pos);
    }
}

public struct Creature
{
    private readonly Creatures creatures;
    private readonly int index;
    public Creature(Creatures creatures, int index)
    {
        this.creatures = creatures;
        this.index = index;
    }
    public ref byte Age => ref creatures.age[index];
    public ref int X => ref creatures.x[index];
    public ref int Y => ref creatures.y[index];
}

 // AoS
 var creatures = new Creature[100];
 foreach (var c in creatures)
 {
	 c.X++; // not memory-efficient
 }
 // SoA
 var creatures2 = new Creatures(100);
 foreach (var c in creatures2)
 {
	 c.X++; // better!
 }
```

##### Composite Proxy with Array-Backed Properties
```cs
public class MasonrySettings
{
	 private bool[] flags = new bool[3];
	 public bool Pillars
	 {
		 get => flags[0];
		 set => flags[0] = value;
	 }

	 public bool Floors
	 {
		 get => flags[1];
		 set => flags[1] = value;
	 }
	 public bool Walls
	 {
		 get => flags[2];
		 set => flags[2] = value;
	 }
	 // similar for Floors and Walls

	public bool? All
	{
	 get
	 {
		 if (flags.Skip(1).All(f => f == flags[0]))
			 return flags[0];
		 return null;
	 }
	 set
	 {
		 if (!value.HasValue) return;
		 for (int i = 0; i < flags.Length; ++i)
			 flags[i] = value.Value;
	 }
	}
}
```

##### Virtual Proxy
An object that has the same API as the original, giving the appearance of an instantiated object, but behind the scenes the proxy only instantiates the object when it’s actually necessary.

```cs
interface IImage
{
 void Draw();
}

class LazyBitmap : IImage
{
 private readonly string filename;
 private Bitmap bitmap;
 public LazyBitmap(string filename)
 {
	 this.filename = filename;
 }
 public void Draw()
 {
	 if (bitmap == null) // lazy loading
		 bitmap = new Bitmap(filename);
		 bitmap.Draw();
	 }
}

var img = new LazyBitmap("pokemon.png");
WriteLine("About to draw the image");
img.Draw(); // image loaded here
WriteLine("Done drawing the image");
```

##### Communication Proxy
A component that proxies the calls “over the wire” and of course collects results, if necessary.
```cs
interface IPingable
{
 string Ping(string message);
}

class Pong : IPingable
{
	public string Ping(string message)
	{
		return message + " pong";
	}
}

class RemotePong : IPingable
{
 string Ping(string message)
 {
	 string uri = "http://localhost:9149/api/pingpong/" + message;
	 return new WebClient().DownloadString(uri);
 }
}
```

##### Composite Proxy
```cs
public class PriorityStringBuilder
{
 private readonly int defaultPriority;
 private readonly SortedDictionary<int, StringBuilder> data = new ();
 private StringBuilder sb => data[defaultPriority];
 public PriorityStringBuilder(int defaultPriority = 100)
 {
	 this.defaultPriority = defaultPriority;
	 data[defaultPriority] = new StringBuilder();
 }
 // generated members here
 public StringBuilder this[int priority]
	{
	 get
	 {
		 if (!data.ContainsKey(priority))
			 data.Add(priority, new StringBuilder());
		 return data[priority];
	 }
	}

	public override string ToString()
	{
		 return string.Join(string.Empty, data.Values.Select(s => s.ToString()));
	}
}

var b = new PriorityStringBuilder();
b.AppendLine("<p>Hello, World!</p>");
b[50].AppendLine("<html>").AppendLine("<body>");
b[150].AppendLine("</body").AppendLine("</html>");
Console.WriteLine(b);
```

#### Chain of Responsibility
**Chain of Responsibility** is a behavioral design pattern that lets you pass requests along a chain of handlers. Upon receiving a request, each handler decides either to process the request or to pass it to the next handler in the chain.

- **Use the Chain of Responsibility pattern when your program is expected to process different kinds of requests in various ways, but the exact types of requests and their sequences are unknown beforehand.** The pattern lets you link several handlers into one chain and, upon receiving a request, “ask” each handler whether it can process it. This way all handlers get a chance to process the request.
- **Use the pattern when it’s essential to execute several handlers in a particular order.** Since you can link the handlers in the chain in any order, all requests will get through the chain exactly as you planned.
- **Use the CoR pattern when the set of handlers and their order are supposed to change at runtime.** If you provide setters for a reference field inside the handler classes, you’ll be able to insert, remove or reorder handlers dynamically.

##### Conceptual Example
```cs
// The Handler interface declares a method for building the chain of
// handlers. It also declares a method for executing a request.
public interface IHandler
{
    IHandler SetNext(IHandler handler);

    object Handle(object request);
}

// The default chaining behavior can be implemented inside a base handler
// class.
abstract class AbstractHandler : IHandler
{
    private IHandler _nextHandler;

    public IHandler SetNext(IHandler handler)
    {
        this._nextHandler = handler;

        // Returning a handler from here will let us link handlers in a
        // convenient way like this:
        // monkey.SetNext(squirrel).SetNext(dog);
        return handler;
    }

    public virtual object Handle(object request)
    {
        if (this._nextHandler != null)
        {
            return this._nextHandler.Handle(request);
        }
        else
        {
            return null;
        }
    }
}

class MonkeyHandler : AbstractHandler
{
    public override object Handle(object request)
    {
        if ((request as string) == "Banana")
        {
            return $"Monkey: I'll eat the {request.ToString()}.\n";
        }
        else
        {
            return base.Handle(request);
        }
    }
}

class SquirrelHandler : AbstractHandler
{
    public override object Handle(object request)
    {
        if (request.ToString() == "Nut")
        {
            return $"Squirrel: I'll eat the {request.ToString()}.\n";
        }
        else
        {
            return base.Handle(request);
        }
    }
}

class DogHandler : AbstractHandler
{
    public override object Handle(object request)
    {
        if (request.ToString() == "MeatBall")
        {
            return $"Dog: I'll eat the {request.ToString()}.\n";
        }
        else
        {
            return base.Handle(request);
        }
    }
}

class Client
{
    // The client code is usually suited to work with a single handler. In
    // most cases, it is not even aware that the handler is part of a chain.
    public static void ClientCode(AbstractHandler handler)
    {
        foreach (var food in new List<string> { "Nut", "Banana", "Cup of coffee" })
        {
            Console.WriteLine($"Client: Who wants a {food}?");

            var result = handler.Handle(food);

            if (result != null)
            {
                Console.Write($"   {result}");
            }
            else
            {
                Console.WriteLine($"   {food} was left untouched.");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        // The other part of the client code constructs the actual chain.
        var monkey = new MonkeyHandler();
        var squirrel = new SquirrelHandler();
        var dog = new DogHandler();

        monkey.SetNext(squirrel).SetNext(dog);

        // The client should be able to send a request to any handler, not
        // just the first one in the chain.
        Console.WriteLine("Chain: Monkey > Squirrel > Dog\n");
        Client.ClientCode(monkey);

        Console.WriteLine("Subchain: Squirrel > Dog\n");
        Client.ClientCode(squirrel);
    }
}
```

##### Method Chain
```cs
public class CreatureModifier
{
 protected Creature creature;
 protected CreatureModifier next;
 public CreatureModifier(Creature creature)
 {
	 this.creature = creature;
 }
 public void Add(CreatureModifier cm)
 {
	 if (next != null) next.Add(cm);
	 else next = cm;
 }
 public virtual void Handle() => next?.Handle();
}

public class DoubleAttackModifier : CreatureModifier
{
 public DoubleAttackModifier(Creature creature) : base(creature) {}
 public override void Handle()
 {
	 WriteLine($"Doubling {creature.Name}'s attack");
	 creature.Attack *= 2;
	 base.Handle();
 }
}

var goblin = new Creature("Goblin", 1, 1);
WriteLine(goblin); // Name: Goblin, Attack: 1, Defense: 1
var root = new CreatureModifier(goblin);
root.Add(new DoubleAttackModifier(goblin));
root.Add(new DoubleAttackModifier(goblin));
```

##### Broker Chain
```cs
public class Game // mediator pattern
{
	 public event EventHandler<Query> Queries; // effectively a chain
	 public void PerformQuery(object sender, Query q)
	 {
		 Queries?.Invoke(sender, q);
	 }
}

public class Query
{
	 public string CreatureName;
	 public enum Argument
	 {
		 Attack, Defense
	 }
	 public Argument WhatToQuery;
	 public int Value; // bidirectional!
}

public class Creature
{
	 private Game game;
	 public string Name;
	 private int attack, defense;
	 public Creature(Game game, string name, int attack, int defense)
	 {
	 // obvious stuff here
	 }

	  public int Attack
		{
		 get
		 {
			 var q = new Query(Name, Query.Argument.Attack, attack);
			 game.PerformQuery(this, q);
			 return q.Value;
		 }
		}
}

public abstract class CreatureModifier : IDisposable
{
	 protected Game game;
	 protected Creature creature;
	 protected CreatureModifier(Game game, Creature creature)
	 {
		 this.game = game;
		 this.creature = creature;
		 game.Queries += Handle; // subscribe
	 }
	 protected abstract void Handle(object sender, Query q);
	 public void Dispose()
	 {
		 game.Queries -= Handle; // unsubscribe
	 }
}

public class DoubleAttackModifier : CreatureModifier
{
	 public DoubleAttackModifier(Game game, Creature creature) : base(game, creature) {}
	 protected override void Handle(object sender, Query q)
	 {
		 if (q.CreatureName == creature.Name && q.WhatToQuery == Query.Argument.Attack)
			 q.Value *= 2;
	 }
}

var game = new Game();
var goblin = new Creature(game, "Strong Goblin", 2, 2);
WriteLine(goblin); // Name: Strong Goblin, attack: 2, defense: 2
using (new DoubleAttackModifier(game, goblin))
{
 WriteLine(goblin); // Name: Strong Goblin, attack: 4, defense: 2
 using (new IncreaseDefenseModifier(game, goblin))
 {
	 WriteLine(goblin); // Name: Strong Goblin, attack: 4, defense: 4
 }
}
WriteLine(goblin); // Name: Strong Goblin, attack: 2, defense: 2
```

#### Command
**Command** is a behavioral design pattern that turns a request into a stand-alone object that contains all information about the request. This transformation lets you pass requests as a method arguments, delay or queue a request’s execution, and support undoable operations.

- **Use the Command pattern when you want to parametrize objects with operations.** The Command pattern can turn a specific method call into a stand-alone object. This change opens up a lot of interesting uses: you can pass commands as method arguments, store them inside other objects, switch linked commands at runtime, etc. Here’s an example: you’re developing a GUI component such as a context menu, and you want your users to be able to configure menu items that trigger operations when an end user clicks an item.
- **Use the Command pattern when you want to queue operations, schedule their execution, or execute them remotely.** As with any other object, a command can be serialized, which means converting it to a string that can be easily written to a file or a database. Later, the string can be restored as the initial command object. Thus, you can delay and schedule command execution. But there’s even more! In the same way, you can queue, log or send commands over the network.
- **Use the Command pattern when you want to implement reversible operations.** Although there are many ways to implement undo/redo, the Command pattern is perhaps the most popular of all. To be able to revert operations, you need to implement the history of performed operations. The command history is a stack that contains all executed command objects along with related backups of the application’s state. This method has two drawbacks. First, it isn’t that easy to save an application’s state because some of it can be private. This problem can be mitigated with the Memento pattern. Second, the state backups may consume quite a lot of RAM. Therefore, sometimes you can resort to an alternative implementation: instead of restoring the past state, the command performs the inverse operation. The reverse operation also has a price: it may turn out to be hard or even impossible to implement.

##### Conceptual Example
```cs
// The Command interface declares a method for executing a command.
public interface ICommand
{
    void Execute();
}

// Some commands can implement simple operations on their own.
class SimpleCommand : ICommand
{
    private string _payload = string.Empty;

    public SimpleCommand(string payload)
    {
        this._payload = payload;
    }

    public void Execute()
    {
        Console.WriteLine($"SimpleCommand: See, I can do simple things like printing ({this._payload})");
    }
}

// However, some commands can delegate more complex operations to other
// objects, called "receivers."
class ComplexCommand : ICommand
{
    private Receiver _receiver;

    // Context data, required for launching the receiver's methods.
    private string _a;

    private string _b;

    // Complex commands can accept one or several receiver objects along
    // with any context data via the constructor.
    public ComplexCommand(Receiver receiver, string a, string b)
    {
        this._receiver = receiver;
        this._a = a;
        this._b = b;
    }

    // Commands can delegate to any methods of a receiver.
    public void Execute()
    {
        Console.WriteLine("ComplexCommand: Complex stuff should be done by a receiver object.");
        this._receiver.DoSomething(this._a);
        this._receiver.DoSomethingElse(this._b);
    }
}

// The Receiver classes contain some important business logic. They know how
// to perform all kinds of operations, associated with carrying out a
// request. In fact, any class may serve as a Receiver.
class Receiver
{
    public void DoSomething(string a)
    {
        Console.WriteLine($"Receiver: Working on ({a}.)");
    }

    public void DoSomethingElse(string b)
    {
        Console.WriteLine($"Receiver: Also working on ({b}.)");
    }
}

// The Invoker is associated with one or several commands. It sends a
// request to the command.
class Invoker
{
    private ICommand _onStart;

    private ICommand _onFinish;

    // Initialize commands.
    public void SetOnStart(ICommand command)
    {
        this._onStart = command;
    }

    public void SetOnFinish(ICommand command)
    {
        this._onFinish = command;
    }

    // The Invoker does not depend on concrete command or receiver classes.
    // The Invoker passes a request to a receiver indirectly, by executing a
    // command.
    public void DoSomethingImportant()
    {
        Console.WriteLine("Invoker: Does anybody want something done before I begin?");
        if (this._onStart is ICommand)
        {
            this._onStart.Execute();
        }

        Console.WriteLine("Invoker: ...doing something really important...");

        Console.WriteLine("Invoker: Does anybody want something done after I finish?");
        if (this._onFinish is ICommand)
        {
            this._onFinish.Execute();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        // The client code can parameterize an invoker with any commands.
        Invoker invoker = new Invoker();
        invoker.SetOnStart(new SimpleCommand("Say Hi!"));
        Receiver receiver = new Receiver();
        invoker.SetOnFinish(new ComplexCommand(receiver, "Send email", "Save report"));

        invoker.DoSomethingImportant();
    }
}
```

##### Implementing the Command Pattern
```cs
public interface ICommand
{
	void Call();
	void Undo();
}

public class BankAccountCommand : ICommand
{
	private BankAccount account;
	public enum Action
	{
		Deposit, Withdraw
	}
	private Action action;
	private int amount;
	private bool succeeded;

	public BankAccountCommand(BankAccount account, Action action, int amount) { }

	public void Call()
	{
		switch (action)
		{
			case Action.Deposit:
				account.Deposit(amount);
				succeeded = true;
				break;
			case Action.Withdraw:
				succeeded = account.Withdraw(amount);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	 public void Undo()
	 {
	   if (!succeeded) return;
		 switch (action)
		 {
			 case Action.Deposit:
				 account.Withdraw(amount);
				 break;
			 case Action.Withdraw:
				 account.Deposit(amount);
				 break;
			 default:
				 throw new ArgumentOutOfRangeException();
		 }
	 }
}

var ba = new BankAccount();
var cmdDeposit = new BankAccountCommand(ba,
BankAccountCommand.Action.Deposit,  100);
var cmdWithdraw = new BankAccountCommand(ba,
BankAccountCommand.Action.Withdraw,  1000);
cmdDeposit.Call();
cmdWithdraw.Call();
WriteLine(ba); // balance: 100
cmdWithdraw.Undo();
cmdDeposit.Undo();
WriteLine(ba); // balance: 0
```

##### Composite Commands (aka Macros)
```cs
abstract class CompositeBankAccountCommand : List<BankAccountCommand>, ICommand
{
 public virtual void Call()
 {
	 ForEach(cmd => cmd.Call());
 }
 public virtual void Undo()
 {
	 foreach (var cmd in ((IEnumerable<BankAccountCommand>)this).Reverse())
	 {
		 cmd.Undo();
	 }
 }
}

class MoneyTransferCommand : CompositeBankAccountCommand
{
	public MoneyTransferCommand(BankAccount from, BankAccount to, int amount)
	{
	 AddRange(new []
	 {
		 new BankAccountCommand(from, BankAccountCommand.Action.Withdraw, amount),
		 new BankAccountCommand(to, BankAccountCommand.Action.Deposit, amount)
	 });
	}

	public override void Call()
	{
	 bool ok = true;
	 foreach (var cmd in this)
	 {
		 if (ok)
		 {
			 cmd.Call();
			 ok = cmd.Success;
		 }
		 else
		 {
			 cmd.Success = false;
		 }
	 }
	}
}
```

##### Functional Command
```cs
public class BankAccount
{
	 public int Balance;
}

public void Deposit(BankAccount account, int amount)
{
	account.Balance += amount;
}
public void Withdraw(BankAccount account, int amount)
{
	if (account.Balance >= amount)
		account.Balance -= amount;
}

var ba = new BankAccount();
var commands = new List<Action>();
commands.Add(() => Deposit(ba, 100));
commands.Add(() => Withdraw(ba, 100));
commands.ForEach(c => c());
```

#### Iterator
**Iterator** is a behavioral design pattern that lets you traverse elements of a collection without exposing its underlying representation (list, stack, tree, etc.).

- **Use the Iterator pattern when your collection has a complex data structure under the hood, but you want to hide its complexity from clients (either for convenience or security reasons).** The iterator encapsulates the details of working with a complex data structure, providing the client with several simple methods of accessing the collection elements. While this approach is very convenient for the client, it also protects the collection from careless or malicious actions which the client would be able to perform if working with the collection directly.
- **Use the pattern to reduce duplication of the traversal code across your app.** The code of non-trivial iteration algorithms tends to be very bulky. When placed within the business logic of an app, it may blur the responsibility of the original code and make it less maintainable. Moving the traversal code to designated iterators can help you make the code of the application more lean and clean.
- **Use the Iterator when you want your code to be able to traverse different data structures or when types of these structures are unknown beforehand.** The pattern provides a couple of generic interfaces for both collections and iterators. Given that your code now uses these interfaces, it’ll still work if you pass it various kinds of collections and iterators that implement these interfaces.

##### Array-Backed Properties
```cs
public class Creature : IEnumerable<int>
{
    public int Agility { get; set; }
    public int Intelligence { get; set; }
    public double AverageStat => _stats.Average();
    public double SumOfStats => _stats.Sum();
    public double MaxStat => _stats.Max();
    private int[] _stats = new int[3];
    private const int strength = 0;
    public ref int Strength => ref _stats[strength];
    public IEnumerable<int> Stats => _stats.AsReadOnly();
    // as before
    public IEnumerator<int> GetEnumerator() => _stats.AsEnumerable().GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ref int this[int index] => ref _stats[index];
}
```

##### Improved Iterator
```cs
public class Node<T>
{
    public T Value;
    public Node<T> Left, Right, Parent;
    public Node(T value)
    {
        Value = value;
    }
    public Node(T value, Node<T> left, Node<T> right)
    {
        Value = value;
        Left = left;
        Right = right;
        left.Parent = right.Parent = this;
    }

}

public class InOrderIterator<T>
{
    public Node<T> Current { get; set; }
    private readonly Node<T> root;
    private bool yieldedStart;
    public InOrderIterator(Node<T> root)
    {
        this.root = Current = root;
        while (Current.Left != null)
            Current = Current.Left;
    }
    public bool MoveNext()
    {
        if (!yieldedStart)
        {
            yieldedStart = true;
            return true;
        }

        if (Current.Right != null)
        {
            Current = Current.Right;
            while (Current.Left != null)
                Current = Current.Left;
            return true;
        }
        else
        {
            var p = Current.Parent;
            while (p != null && Current == p.Right)
            {
                Current = p;
                p = p.Parent;
            }
            Current = p;
            return Current != null;
        }

    }

    public IEnumerable<Node<T>> NaturalInOrder
    {
        get
        {
            IEnumerable<Node<T>> TraverseInOrder(Node<T> current)
            {
                if (current.Left != null)
                {
                    foreach (var left in TraverseInOrder(current.Left))
                        yield return left;
                }
                yield return current;
                if (current.Right != null)
                {
                    foreach (var right in TraverseInOrder(current.Right))
                        yield return right;
                }
            }
            foreach (var node in TraverseInOrder(root))
                yield return node;
        }
    }
}

public class BinaryTree<T>
{
    private Node<T> root;
    public BinaryTree(Node<T> root)
    {
        this.root = root;
    }
    public InOrderIterator<T> GetEnumerator()
    {
        return new InOrderIterator<T>(root);
    }
}

var root = new Node<int>(1,
new Node<int>(2), new Node<int>(3));
var tree = new BinaryTree<int>(root);
WriteLine(string.Join(",", tree.NaturalInOrder.Select(x => x.Value)));
// 2,1,3
```

##### Iterator Adapter
```cs
public class OneDAdapter<T> : IEnumerable<T>
{
    private readonly T[,] arr;
    private int w, h;
    public OneDAdapter(T[,] arr)
    {
        this.arr = arr;
        w = arr.GetLength(0);
        h = arr.GetLength(1);
    }
    public IEnumerator<T> GetEnumerator()
    {
        for (int y = 0; y < h; ++y)
            for (int x = 0; x < w; ++x)
                yield return arr[x, y];
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class ReverseIterable<T> : IEnumerable<T>
{
    private readonly T[] arr;
    public ReverseIterable(T[] arr) => this.arr = arr;
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = arr.Length - 1; i >= 0; --i)
            yield return arr[i];
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class ReverseIterable
{
    public static ReverseIterable<T> From<T>(T[] arr)
    {
        return new ReverseIterable<T>(arr);
    }
}
```

#### Mediator
**Mediator** is a behavioral design pattern that lets you reduce chaotic dependencies between objects. The pattern restricts direct communications between the objects and forces them to collaborate only via a mediator object.

- **Use the Mediator pattern when it’s hard to change some of the classes because they are tightly coupled to a bunch of other classes.** The pattern lets you extract all the relationships between classes into a separate class, isolating any changes to a specific component from the rest of the components.
- **Use the pattern when you can’t reuse a component in a different program because it’s too dependent on other components.** After you apply the Mediator, individual components become unaware of the other components. They could still communicate with each other, albeit indirectly, through a mediator object. To reuse a component in a different app, you need to provide it with a new mediator class.
- **Use the Mediator when you find yourself creating tons of component subclasses just to reuse some basic behavior in various contexts.** Since all relations between components are contained within the mediator, it’s easy to define entirely new ways for these components to collaborate by introducing new mediator classes, without having to change the components themselves.

```cs
// The Mediator interface declares a method used by components to notify the
// mediator about various events. The Mediator may react to these events and
// pass the execution to other components.
public interface IMediator
{
    void Notify(object sender, string ev);
}

// Concrete Mediators implement cooperative behavior by coordinating several
// components.
class ConcreteMediator : IMediator
{
    private Component1 _component1;

    private Component2 _component2;

    public ConcreteMediator(Component1 component1, Component2 component2)
    {
        this._component1 = component1;
        this._component1.SetMediator(this);
        this._component2 = component2;
        this._component2.SetMediator(this);
    }

    public void Notify(object sender, string ev)
    {
        if (ev == "A")
        {
            Console.WriteLine("Mediator reacts on A and triggers following operations:");
            this._component2.DoC();
        }
        if (ev == "D")
        {
            Console.WriteLine("Mediator reacts on D and triggers following operations:");
            this._component1.DoB();
            this._component2.DoC();
        }
    }
}

// The Base Component provides the basic functionality of storing a
// mediator's instance inside component objects.
class BaseComponent
{
    protected IMediator _mediator;

    public BaseComponent(IMediator mediator = null)
    {
        this._mediator = mediator;
    }

    public void SetMediator(IMediator mediator)
    {
        this._mediator = mediator;
    }
}

// Concrete Components implement various functionality. They don't depend on
// other components. They also don't depend on any concrete mediator
// classes.
class Component1 : BaseComponent
{
    public void DoA()
    {
        Console.WriteLine("Component 1 does A.");

        this._mediator.Notify(this, "A");
    }

    public void DoB()
    {
        Console.WriteLine("Component 1 does B.");

        this._mediator.Notify(this, "B");
    }
}

class Component2 : BaseComponent
{
    public void DoC()
    {
        Console.WriteLine("Component 2 does C.");

        this._mediator.Notify(this, "C");
    }

    public void DoD()
    {
        Console.WriteLine("Component 2 does D.");

        this._mediator.Notify(this, "D");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // The client code.
        Component1 component1 = new Component1();
        Component2 component2 = new Component2();
        new ConcreteMediator(component1, component2);

        Console.WriteLine("Client triggers operation A.");
        component1.DoA();

        Console.WriteLine();

        Console.WriteLine("Client triggers operation D.");
        component2.DoD();
    }
}
```

#### Memento
**Memento** is a behavioral design pattern that lets you save and restore the previous state of an object without revealing the details of its implementation.

- **Use the Memento pattern when you want to produce snapshots of the object’s state to be able to restore a previous state of the object.** The Memento pattern lets you make full copies of an object’s state, including private fields, and store them separately from the object. While most people remember this pattern thanks to the “undo” use case, it’s also indispensable when dealing with transactions (i.e., if you need to roll back an operation on error).
- **Use the pattern when direct access to the object’s fields/getters/setters violates its encapsulation.** The Memento makes the object itself responsible for creating a snapshot of its state. No other object can read the snapshot, making the original object’s state data safe and secure.

##### Conceptual Example
```cs
// The Originator holds some important state that may change over time. It
// also defines a method for saving the state inside a memento and another
// method for restoring the state from it.
class Originator
{
    // For the sake of simplicity, the originator's state is stored inside a
    // single variable.
    private string _state;

    public Originator(string state)
    {
        this._state = state;
        Console.WriteLine("Originator: My initial state is: " + state);
    }

    // The Originator's business logic may affect its internal state.
    // Therefore, the client should backup the state before launching
    // methods of the business logic via the save() method.
    public void DoSomething()
    {
        Console.WriteLine("Originator: I'm doing something important.");
        this._state = this.GenerateRandomString(30);
        Console.WriteLine($"Originator: and my state has changed to: {_state}");
    }

    private string GenerateRandomString(int length = 10)
    {
        string allowedSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string result = string.Empty;

        while (length > 0)
        {
            result += allowedSymbols[new Random().Next(0, allowedSymbols.Length)];

            Thread.Sleep(12);

            length--;
        }

        return result;
    }

    // Saves the current state inside a memento.
    public IMemento Save()
    {
        return new ConcreteMemento(this._state);
    }

    // Restores the Originator's state from a memento object.
    public void Restore(IMemento memento)
    {
        if (!(memento is ConcreteMemento))
        {
            throw new Exception("Unknown memento class " + memento.ToString());
        }

        this._state = memento.GetState();
        Console.Write($"Originator: My state has changed to: {_state}");
    }
}

// The Memento interface provides a way to retrieve the memento's metadata,
// such as creation date or name. However, it doesn't expose the
// Originator's state.
public interface IMemento
{
    string GetName();

    string GetState();

    DateTime GetDate();
}

// The Concrete Memento contains the infrastructure for storing the
// Originator's state.
class ConcreteMemento : IMemento
{
    private string _state;

    private DateTime _date;

    public ConcreteMemento(string state)
    {
        this._state = state;
        this._date = DateTime.Now;
    }

    // The Originator uses this method when restoring its state.
    public string GetState()
    {
        return this._state;
    }

    // The rest of the methods are used by the Caretaker to display
    // metadata.
    public string GetName()
    {
        return $"{this._date} / ({this._state.Substring(0, 9)})...";
    }

    public DateTime GetDate()
    {
        return this._date;
    }
}

// The Caretaker doesn't depend on the Concrete Memento class. Therefore, it
// doesn't have access to the originator's state, stored inside the memento.
// It works with all mementos via the base Memento interface.
class Caretaker
{
    private List<IMemento> _mementos = new List<IMemento>();

    private Originator _originator = null;

    public Caretaker(Originator originator)
    {
        this._originator = originator;
    }

    public void Backup()
    {
        Console.WriteLine("\nCaretaker: Saving Originator's state...");
        this._mementos.Add(this._originator.Save());
    }

    public void Undo()
    {
        if (this._mementos.Count == 0)
        {
            return;
        }

        var memento = this._mementos.Last();
        this._mementos.Remove(memento);

        Console.WriteLine("Caretaker: Restoring state to: " + memento.GetName());

        try
        {
            this._originator.Restore(memento);
        }
        catch (Exception)
        {
            this.Undo();
        }
    }

    public void ShowHistory()
    {
        Console.WriteLine("Caretaker: Here's the list of mementos:");

        foreach (var memento in this._mementos)
        {
            Console.WriteLine(memento.GetName());
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Client code.
        Originator originator = new Originator("Super-duper-super-puper-super.");
        Caretaker caretaker = new Caretaker(originator);

        caretaker.Backup();
        originator.DoSomething();

        caretaker.Backup();
        originator.DoSomething();

        caretaker.Backup();
        originator.DoSomething();

        Console.WriteLine();
        caretaker.ShowHistory();

        Console.WriteLine("\nClient: Now, let's rollback!\n");
        caretaker.Undo();

        Console.WriteLine("\n\nClient: Once more!\n");
        caretaker.Undo();

        Console.WriteLine();
    }
}
```

##### Memento and Command
```cs
public class BankAccount
{
	 private int balance;
	 private List<Memento> changes = new();
	 private int current;
	 public BankAccount(int balance)
	 {
		 this.balance = balance;
		 changes.Add(new Memento(balance));
	 }

	public Memento Deposit(int amount)
	{
		 balance += amount;
		 var m = new Memento(balance);
		 changes.Add(m);
		 ++current;
		 return m;
	}

	public void Restore(Memento? m)
	{
		 if (m != null)
		 {
			 balance = m.Balance;
			 changes.Add(m);
			 current = changes.Count - 1;
		 }
	}

	public Memento Undo()
	{
		 if (current > 0)
		 {
			 var m = changes[--current];
			 balance = m.Balance;
			 return m;
		 }
		 return null;
	}

	public Memento Redo()
	{
	 if (current + 1 < changes.Count)
	 {
		 var m = changes[++current];
		 balance = m.Balance;
		 return m;
	 }
	 return null;
	}
}

public class Memento
{
 public int Balance { get; }
 public Memento(int balance)
 {
	 Balance = balance;
 }
}

 public class WithdrawCommand : ICommand
 {
	 public BankAccount Account;
	 public decimal Amount;
	 private Memento memento;
	 private bool succeeded;
	 public override void Call()
	 {
		 succeeded = Account.Withdraw(Amount);
		 memento = Account.Snapshot(); // memento-creating method
	 }
	 public override void Undo()
	 {
		 if (succeeded && memento != null)
		 {
			 Account.RestoreTo(memento);
			 memento = null; // prevent second undo
		 }
	 }
 }

var ba = new BankAccount(100);
ba.Deposit(50);
ba.Deposit(25);
WriteLine(ba);
ba.Undo();
WriteLine($"Undo 1: {ba}"); // Undo 1: 150
ba.Undo();
WriteLine($"Undo 2: {ba}"); // Undo 2: 100
ba.Redo();
WriteLine($"Redo 2: {ba}"); // Redo 2: 150
```

#### Observer
**Observer** is a behavioral design pattern that lets you define a subscription mechanism to notify multiple objects about any events that happen to the object they’re observing.

- **Use the Observer pattern when changes to the state of one object may require changing other objects, and the actual set of objects is unknown beforehand or changes dynamically.** You can often experience this problem when working with classes of the graphical user interface. For example, you created custom button classes, and you want to let the clients hook some custom code to your buttons so that it fires whenever a user presses a button. The Observer pattern lets any object that implements the subscriber interface subscribe for event notifications in publisher objects. You can add the subscription mechanism to your buttons, letting the clients hook up their custom code via custom subscriber classes.
- **Use the pattern when some objects in your app must observe others, but only for a limited time or in specific cases.** The subscription list is dynamic, so subscribers can join or leave the list whenever they need to.

##### Conceptual Example
```cs
public interface IObserver
{
    // Receive update from subject
    void Update(ISubject subject);
}

public interface ISubject
{
    // Attach an observer to the subject.
    void Attach(IObserver observer);

    // Detach an observer from the subject.
    void Detach(IObserver observer);

    // Notify all observers about an event.
    void Notify();
}

// The Subject owns some important state and notifies observers when the
// state changes.
public class Subject : ISubject
{
    // For the sake of simplicity, the Subject's state, essential to all
    // subscribers, is stored in this variable.
    public int State { get; set; } = -0;

    // List of subscribers. In real life, the list of subscribers can be
    // stored more comprehensively (categorized by event type, etc.).
    private List<IObserver> _observers = new List<IObserver>();

    // The subscription management methods.
    public void Attach(IObserver observer)
    {
        Console.WriteLine("Subject: Attached an observer.");
        this._observers.Add(observer);
    }

    public void Detach(IObserver observer)
    {
        this._observers.Remove(observer);
        Console.WriteLine("Subject: Detached an observer.");
    }

    // Trigger an update in each subscriber.
    public void Notify()
    {
        Console.WriteLine("Subject: Notifying observers...");

        foreach (var observer in _observers)
        {
            observer.Update(this);
        }
    }

    // Usually, the subscription logic is only a fraction of what a Subject
    // can really do. Subjects commonly hold some important business logic,
    // that triggers a notification method whenever something important is
    // about to happen (or after it).
    public void SomeBusinessLogic()
    {
        Console.WriteLine("\nSubject: I'm doing something important.");
        this.State = new Random().Next(0, 10);

        Thread.Sleep(15);

        Console.WriteLine("Subject: My state has just changed to: " + this.State);
        this.Notify();
    }
}

// Concrete Observers react to the updates issued by the Subject they had
// been attached to.
class ConcreteObserverA : IObserver
{
    public void Update(ISubject subject)
    {
        if ((subject as Subject).State < 3)
        {
            Console.WriteLine("ConcreteObserverA: Reacted to the event.");
        }
    }
}

class ConcreteObserverB : IObserver
{
    public void Update(ISubject subject)
    {
        if ((subject as Subject).State == 0 || (subject as Subject).State >= 2)
        {
            Console.WriteLine("ConcreteObserverB: Reacted to the event.");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        // The client code.
        var subject = new Subject();
        var observerA = new ConcreteObserverA();
        subject.Attach(observerA);

        var observerB = new ConcreteObserverB();
        subject.Attach(observerB);

        subject.SomeBusinessLogic();
        subject.SomeBusinessLogic();

        subject.Detach(observerB);

        subject.SomeBusinessLogic();
    }
}
```

##### Event Streams
```cs
private class Subscription : IDisposable
{
	 private Person person;
	 public IObserver<Event> Observer;
	 public Subscription(Person person, IObserver<Event> observer)
	 {
		 this.person = person;
		 Observer = observer;
	 }
	 public void Dispose()
	 {
		 person.subscriptions.Remove(this);
	 }
}

public class Event
{
 // anything could be here
}

public class FallsIllEvent : Event
{
	 public string Address;
}

public class Person : IObservable<Event>
{
	 private readonly HashSet<Subscription> subscriptions = new ();
	 public IDisposable Subscribe(IObserver<Event> observer)
	 {
		 var subscription = new Subscription(this, observer);
		 subscriptions.Add(subscription);
		 return subscription;
	 }
	 public void CatchACold()
	 {
		 foreach (var sub in subscriptions)
			 sub.Observer.OnNext(new FallsIllEvent {Address = "123 London Road"});
	 }
		private class Subscription : IDisposable { ... }
}

public class Demo : IObserver<Event>
{
	 static void Main(string[] args)
	 {
		 new Demo();
	 }
	 public Demo()
	 {
		 var person = new Person();
		 var sub = person.Subscribe(this);
	 }
	 public void OnNext(Event value)
	 {
		 if (value is FallsIllEvent args)
			 WriteLine($"A doctor has been called to {args.Address}");
	 }
	 public void OnError(Exception error){}
	 public void OnCompleted(){}
}

person
 .OfType<FallsIllEvent>()
 .Subscribe(args => WriteLine($"A doctor has been called to {args.Address}"));
```

##### Weak Reference Event for memory leaks
This class is specifically designed to allow the listener’s handlers to be garbage-collected even if the source object persists.
```cs
public class Window2
{
 public Window2(Button button)
 {
	 WeakEventManager<Button, EventArgs>.AddHandler(
		 button, nameof(Button.Clicked), ButtonOnClicked);
 }
 // rest of class same as before
}
```

##### Property Observers
```cs
// Basic Change Notification
public delegate void PropertyChangedEventHandler (object sender, PropertyChangedEventArgs e);

public interface INotifyPropertyChanged
{
 /// <summary>Occurs when a property value changes.</summary>
 event PropertyChangedEventHandler PropertyChanged;
}


public class Person : INotifyPropertyChanged
{
 private int age;
 public int Age
 {
	 get => age;
	 set
	 {
		 if (value == age) return;
		 age = value;
		 OnPropertyChanged();
	 }
 }
 public event PropertyChangedEventHandler PropertyChanged;

 [NotifyPropertyChangedInvocator]
 protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
 {
	 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
 }
}
```

```cs
// Bidirectional Bindings
var product = new Product{Name="Book"};
var window = new Window{ProductName = "Book"};
product.PropertyChanged += (sender, eventArgs) =>
{
 if (eventArgs.PropertyName == "Name")
 {

	 Console.WriteLine("Name changed in Product");
	 window.ProductName = product.Name;
 }
};
window.PropertyChanged += (sender, eventArgs) =>
{
 if (eventArgs.PropertyName == "ProductName")
 {
	 Console.WriteLine("Name changed in Window");
	 product.Name = window.ProductName;
 }
};

public sealed class BidirectionalBinding : IDisposable
{
 private bool disposed;
 public BidirectionalBinding(
	 INotifyPropertyChanged first, Expression<Func<object>> firstProperty,
	 INotifyPropertyChanged second, Expression<Func<object>> secondProperty)
 {
	 if (firstProperty.Body is MemberExpression firstExpr && secondProperty.Body is MemberExpression secondExpr)
	 {
		 if (firstExpr.Member is PropertyInfo firstProp && secondExpr.Member is PropertyInfo secondProp)
		 {
			 first.PropertyChanged += (sender, args) =>
			 {
				 if (!disposed)
					 secondProp.SetValue(second, firstProp.GetValue(first));
			 };
			 second.PropertyChanged += (sender, args) =>
			 {
				 if (!disposed)
				 firstProp.SetValue(first, secondProp.GetValue(second));
			 };
		 }
	 }
 }
 public void Dispose()
 {
	 disposed = true;
 }
}
```

#### State

**State** is a behavioral design pattern that lets an object alter its behavior when its internal state changes. It appears as if the object changed its class.

- **Use the State pattern when you have an object that behaves differently depending on its current state, the number of states is enormous, and the state-specific code changes frequently.** The pattern suggests that you extract all state-specific code into a set of distinct classes. As a result, you can add new states or change existing ones independently of each other, reducing the maintenance cost.
- **Use the pattern when you have a class polluted with massive conditionals that alter how the class behaves according to the current values of the class’s fields.** The State pattern lets you extract branches of these conditionals into methods of corresponding state classes. While doing so, you can also clean temporary fields and helper methods involved in state-specific code out of your main class.
- **Use State when you have a lot of duplicate code across similar states and transitions of a condition-based state machine.** The State pattern lets you compose hierarchies of state classes and reduce duplication by extracting common code into abstract base classes.

##### Conceptual Example
```cs
// The Context defines the interface of interest to clients. It also
// maintains a reference to an instance of a State subclass, which
// represents the current state of the Context.
class Context
{
    // A reference to the current state of the Context.
    private State _state = null;

    public Context(State state)
    {
        this.TransitionTo(state);
    }

    // The Context allows changing the State object at runtime.
    public void TransitionTo(State state)
    {
        Console.WriteLine($"Context: Transition to {state.GetType().Name}.");
        this._state = state;
        this._state.SetContext(this);
    }

    // The Context delegates part of its behavior to the current State
    // object.
    public void Request1()
    {
        this._state.Handle1();
    }

    public void Request2()
    {
        this._state.Handle2();
    }
}

// The base State class declares methods that all Concrete State should
// implement and also provides a backreference to the Context object,
// associated with the State. This backreference can be used by States to
// transition the Context to another State.
abstract class State
{
    protected Context _context;

    public void SetContext(Context context)
    {
        this._context = context;
    }

    public abstract void Handle1();

    public abstract void Handle2();
}

// Concrete States implement various behaviors, associated with a state of
// the Context.
class ConcreteStateA : State
{
    public override void Handle1()
    {
        Console.WriteLine("ConcreteStateA handles request1.");
        Console.WriteLine("ConcreteStateA wants to change the state of the context.");
        this._context.TransitionTo(new ConcreteStateB());
    }

    public override void Handle2()
    {
        Console.WriteLine("ConcreteStateA handles request2.");
    }
}

class ConcreteStateB : State
{
    public override void Handle1()
    {
        Console.Write("ConcreteStateB handles request1.");
    }

    public override void Handle2()
    {
        Console.WriteLine("ConcreteStateB handles request2.");
        Console.WriteLine("ConcreteStateB wants to change the state of the context.");
        this._context.TransitionTo(new ConcreteStateA());
    }
}

class Program
{
    static void Main(string[] args)
    {
        // The client code.
        var context = new Context(new ConcreteStateA());
        context.Request1();
        context.Request2();
    }
}
```

##### Encoding Transitions with Switch Expressions
```cs
enum Chest { Open, Closed, Locked }
enum Action { Open, Close }

static Chest Manipulate(Chest chest, Action action, bool haveKey)
	=> (chest, action, haveKey) switch
	 {
		 (Chest.Closed, Action.Open, _) => Chest.Open,
		 (Chest.Locked, Action.Open, true) => Chest.Open,
		 (Chest.Open, Action.Close, true) => Chest.Locked,
		 (Chest.Open, Action.Close, false) => Chest.Closed,
		 _ => chest
	 };
```

#### Strategy
**Strategy** is a behavioral design pattern that lets you define a family of algorithms, put each of them into a separate class, and make their objects interchangeable.

- **Use the Strategy pattern when you want to use different variants of an algorithm within an object and be able to switch from one algorithm to another during runtime.** The Strategy pattern lets you indirectly alter the object’s behavior at runtime by associating it with different sub-objects which can perform specific sub-tasks in different ways.
- **Use the Strategy when you have a lot of similar classes that only differ in the way they execute some behavior.** The Strategy pattern lets you extract the varying behavior into a separate class hierarchy and combine the original classes into one, thereby reducing duplicate code.
- **Use the pattern to isolate the business logic of a class from the implementation details of algorithms that may not be as important in the context of that logic.** The Strategy pattern lets you isolate the code, internal data, and dependencies of various algorithms from the rest of the code. Various clients get a simple interface to execute the algorithms and switch them at runtime.
- **Use the pattern when your class has a massive conditional statement that switches between different variants of the same algorithm.** The Strategy pattern lets you do away with such a conditional by extracting all algorithms into separate classes, all of which implement the same interface. The original object delegates execution to one of these objects, instead of implementing all variants of the algorithm.

##### Conceptual Example
```cs
// The Context defines the interface of interest to clients.
class Context
{
    // The Context maintains a reference to one of the Strategy objects. The
    // Context does not know the concrete class of a strategy. It should
    // work with all strategies via the Strategy interface.
    private IStrategy _strategy;

    public Context()
    { }

    // Usually, the Context accepts a strategy through the constructor, but
    // also provides a setter to change it at runtime.
    public Context(IStrategy strategy)
    {
        this._strategy = strategy;
    }

    // Usually, the Context allows replacing a Strategy object at runtime.
    public void SetStrategy(IStrategy strategy)
    {
        this._strategy = strategy;
    }

    // The Context delegates some work to the Strategy object instead of
    // implementing multiple versions of the algorithm on its own.
    public void DoSomeBusinessLogic()
    {
        Console.WriteLine("Context: Sorting data using the strategy (not sure how it'll do it)");
        var result = this._strategy.DoAlgorithm(new List<string> { "a", "b", "c", "d", "e" });

        string resultStr = string.Empty;
        foreach (var element in result as List<string>)
        {
            resultStr += element + ",";
        }

        Console.WriteLine(resultStr);
    }
}

// The Strategy interface declares operations common to all supported
// versions of some algorithm.
//
// The Context uses this interface to call the algorithm defined by Concrete
// Strategies.
public interface IStrategy
{
    object DoAlgorithm(object data);
}

// Concrete Strategies implement the algorithm while following the base
// Strategy interface. The interface makes them interchangeable in the
// Context.
class ConcreteStrategyA : IStrategy
{
    public object DoAlgorithm(object data)
    {
        var list = data as List<string>;
        list.Sort();

        return list;
    }
}

class ConcreteStrategyB : IStrategy
{
    public object DoAlgorithm(object data)
    {
        var list = data as List<string>;
        list.Sort();
        list.Reverse();

        return list;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // The client code picks a concrete strategy and passes it to the
        // context. The client should be aware of the differences between
        // strategies in order to make the right choice.
        var context = new Context();

        Console.WriteLine("Client: Strategy is set to normal sorting.");
        context.SetStrategy(new ConcreteStrategyA());
        context.DoSomeBusinessLogic();

        Console.WriteLine();

        Console.WriteLine("Client: Strategy is set to reverse sorting.");
        context.SetStrategy(new ConcreteStrategyB());
        context.DoSomeBusinessLogic();
    }
}
```

##### Dynamic Strategy
```cs
public enum OutputFormat
{
    Markdown,
    Html
}

public interface IListStrategy
{
    void Start(StringBuilder sb);
    void AddListItem(StringBuilder sb, string item);
    void End(StringBuilder sb);
}

public class TextProcessor
{
    private StringBuilder sb = new StringBuilder();
    private IListStrategy listStrategy;
    public void AppendList(IEnumerable<string> items)
    {
        listStrategy.Start(sb);
        foreach (var item in items)
            listStrategy.AddListItem(sb, item);
        listStrategy.End(sb);
    }
    public override string ToString() => sb.ToString();

    public void SetOutputFormat(OutputFormat format)
    {
        switch (format)
        {
            case OutputFormat.Markdown:
                listStrategy = new MarkdownListStrategy();
                break;
            case OutputFormat.Html:
                listStrategy = new HtmlListStrategy();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}

public class HtmlListStrategy : IListStrategy
{
    public void Start(StringBuilder sb) => sb.AppendLine("<ul>");
    public void End(StringBuilder sb) => sb.AppendLine("</ul>");
    public void AddListItem(StringBuilder sb, string item)
    {
        sb.AppendLine($"  <li>{item}</li>");
    }
}

public class MarkdownListStrategy : IListStrategy
{
    // markdown doesn't require list start/end tags
    public void Start(StringBuilder sb) { }
    public void End(StringBuilder sb) { }
    public void AddListItem(StringBuilder sb, string item)
    {
        sb.AppendLine($" * {item}");
    }
}


var tp = new TextProcessor();
tp.SetOutputFormat(OutputFormat.Markdown);
tp.AppendList(new[] { "foo", "bar", "baz" });
WriteLine(tp);
tp.Clear(); // erases underlying buffer
tp.SetOutputFormat(OutputFormat.Html);
tp.AppendList(new[] { "foo", "bar", "baz" });
WriteLine(tp);
```

##### Static Strategy
```cs
public enum OutputFormat
{
    Markdown,
    Html
}

public interface IListStrategy
{
    void Start(StringBuilder sb);
    void AddListItem(StringBuilder sb, string item);
    void End(StringBuilder sb);
}

public class TextProcessor<LS> where LS : IListStrategy, new()
{
    private StringBuilder sb = new StringBuilder();
    private IListStrategy listStrategy = new LS();
    public void AppendList(IEnumerable<string> items)
    {
        listStrategy.Start(sb);
        foreach (var item in items)
            listStrategy.AddListItem(sb, item);
        listStrategy.End(sb);
    }
    public override string ToString() => return sb.ToString();
 }

public class HtmlListStrategy : IListStrategy
{
    public void Start(StringBuilder sb) => sb.AppendLine("<ul>");
    public void End(StringBuilder sb) => sb.AppendLine("</ul>");
    public void AddListItem(StringBuilder sb, string item)
    {
        sb.AppendLine($"  <li>{item}</li>");
    }
}

public class MarkdownListStrategy : IListStrategy
{
    // markdown doesn't require list start/end tags
    public void Start(StringBuilder sb) { }
    public void End(StringBuilder sb) { }
    public void AddListItem(StringBuilder sb, string item)
    {
        sb.AppendLine($" * {item}");
    }
}


var tp = new TextProcessor<MarkdownListStrategy>();
tp.AppendList(new[] { "foo", "bar", "baz" });
WriteLine(tp);
var tp2 = new TextProcessor<HtmlListStrategy>();
tp2.AppendList(new[] { "foo", "bar", "baz" });
WriteLine(tp2)
```

##### Equality and Comparison Strategies
```cs
public class Person
{
 // ... other members here
 private sealed class NameRelationalComparer : IComparer<Person>
 {
	 public int Compare(Person x, Person y)
	 {
		 if (ReferenceEquals(x, y)) return 0;
		 if (ReferenceEquals(null, y)) return 1;
		 if (ReferenceEquals(null, x)) return -1;
		 return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
	 }
	}
 public static IComparer<Person> NameComparer { get; } = new NameRelationalComparer();
}

people.Sort(Person.NameComparer);
```

#### Template Method
**Template Method** is a behavioral design pattern that defines the skeleton of an algorithm in the superclass but lets subclasses override specific steps of the algorithm without changing its structure.

- **Use the Template Method pattern when you want to let clients extend only particular steps of an algorithm, but not the whole algorithm or its structure.** The Template Method lets you turn a monolithic algorithm into a series of individual steps which can be easily extended by subclasses while keeping intact the structure defined in a superclass.
- **Use the pattern when you have several classes that contain almost identical algorithms with some minor differences. As a result, you might need to modify all classes when the algorithm changes.** When you turn such an algorithm into a template method, you can also pull up the steps with similar implementations into a superclass, eliminating code duplication. Code that varies between subclasses can remain in subclasses.

##### Conceptual Example
```cs
// The Abstract Class defines a template method that contains a skeleton of
// some algorithm, composed of calls to (usually) abstract primitive
// operations.
//
// Concrete subclasses should implement these operations, but leave the
// template method itself intact.
abstract class AbstractClass
{
    // The template method defines the skeleton of an algorithm.
    public void TemplateMethod()
    {
        this.BaseOperation1();
        this.RequiredOperations1();
        this.BaseOperation2();
        this.Hook1();
        this.RequiredOperation2();
        this.BaseOperation3();
        this.Hook2();
    }

    // These operations already have implementations.
    protected void BaseOperation1()
    {
        Console.WriteLine("AbstractClass says: I am doing the bulk of the work");
    }

    protected void BaseOperation2()
    {
        Console.WriteLine("AbstractClass says: But I let subclasses override some operations");
    }

    protected void BaseOperation3()
    {
        Console.WriteLine("AbstractClass says: But I am doing the bulk of the work anyway");
    }

    // These operations have to be implemented in subclasses.
    protected abstract void RequiredOperations1();

    protected abstract void RequiredOperation2();

    // These are "hooks." Subclasses may override them, but it's not
    // mandatory since the hooks already have default (but empty)
    // implementation. Hooks provide additional extension points in some
    // crucial places of the algorithm.
    protected virtual void Hook1() { }

    protected virtual void Hook2() { }
}

// Concrete classes have to implement all abstract operations of the base
// class. They can also override some operations with a default
// implementation.
class ConcreteClass1 : AbstractClass
{
    protected override void RequiredOperations1()
    {
        Console.WriteLine("ConcreteClass1 says: Implemented Operation1");
    }

    protected override void RequiredOperation2()
    {
        Console.WriteLine("ConcreteClass1 says: Implemented Operation2");
    }
}

// Usually, concrete classes override only a fraction of base class'
// operations.
class ConcreteClass2 : AbstractClass
{
    protected override void RequiredOperations1()
    {
        Console.WriteLine("ConcreteClass2 says: Implemented Operation1");
    }

    protected override void RequiredOperation2()
    {
        Console.WriteLine("ConcreteClass2 says: Implemented Operation2");
    }

    protected override void Hook1()
    {
        Console.WriteLine("ConcreteClass2 says: Overridden Hook1");
    }
}

class Client
{
    // The client code calls the template method to execute the algorithm.
    // Client code does not have to know the concrete class of an object it
    // works with, as long as it works with objects through the interface of
    // their base class.
    public static void ClientCode(AbstractClass abstractClass)
    {
        // ...
        abstractClass.TemplateMethod();
        // ...
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Same client code can work with different subclasses:");

        Client.ClientCode(new ConcreteClass1());

        Console.Write("\n");

        Console.WriteLine("Same client code can work with different subclasses:");
        Client.ClientCode(new ConcreteClass2());
    }
}
```

#### Visitor

**Visitor** is a behavioral design pattern that lets you separate algorithms from the objects on which they operate.

- **Use the Visitor when you need to perform an operation on all elements of a complex object structure (for example, an object tree).** The Visitor pattern lets you execute an operation over a set of objects with different classes by having a visitor object implement several variants of the same operation, which correspond to all target classes.
- **Use the Visitor to clean up the business logic of auxiliary behaviors.** The pattern lets you make the primary classes of your app more focused on their main jobs by extracting all other behaviors into a set of visitor classes.
- **Use the pattern when a behavior makes sense only in some classes of a class hierarchy, but not in others.** You can extract this behavior into a separate visitor class and implement only those visiting methods that accept objects of relevant classes, leaving the rest empty.

##### Conceptual Example
```cs
// The Component interface declares an `accept` method that should take the
// base visitor interface as an argument.
public interface IComponent
{
    void Accept(IVisitor visitor);
}

// Each Concrete Component must implement the `Accept` method in such a way
// that it calls the visitor's method corresponding to the component's
// class.
public class ConcreteComponentA : IComponent
{
    // Note that we're calling `VisitConcreteComponentA`, which matches the
    // current class name. This way we let the visitor know the class of the
    // component it works with.
    public void Accept(IVisitor visitor)
    {
        visitor.VisitConcreteComponent(this);
    }

    // Concrete Components may have special methods that don't exist in
    // their base class or interface. The Visitor is still able to use these
    // methods since it's aware of the component's concrete class.
    public string ExclusiveMethodOfConcreteComponentA()
    {
        return "A";
    }
}

public class ConcreteComponentB : IComponent
{
    // Same here: VisitConcreteComponentB => ConcreteComponentB
    public void Accept(IVisitor visitor)
    {
        visitor.VisitConcreteComponent(this);
    }

    public string SpecialMethodOfConcreteComponentB()
    {
        return "B";
    }
}

// The Visitor Interface declares a set of visiting methods that correspond
// to component classes. The signature of a visiting method allows the
// visitor to identify the exact class of the component that it's dealing
// with.
public interface IVisitor
{
    void VisitConcreteComponent(ConcreteComponentA element);

    void VisitConcreteComponent(ConcreteComponentB element);
}

// Concrete Visitors implement several versions of the same algorithm, which
// can work with all concrete component classes.
//
// You can experience the biggest benefit of the Visitor pattern when using
// it with a complex object structure, such as a Composite tree. In this
// case, it might be helpful to store some intermediate state of the
// algorithm while executing visitor's methods over various objects of the
// structure.
class ConcreteVisitor1 : IVisitor
{
    public void VisitConcreteComponent(ConcreteComponentA element)
    {
        Console.WriteLine(element.ExclusiveMethodOfConcreteComponentA() + " + ConcreteVisitor1");
    }

    public void VisitConcreteComponent(ConcreteComponentB element)
    {
        Console.WriteLine(element.SpecialMethodOfConcreteComponentB() + " + ConcreteVisitor1");
    }
}

class ConcreteVisitor2 : IVisitor
{
    public void VisitConcreteComponent(ConcreteComponentA element)
    {
        Console.WriteLine(element.ExclusiveMethodOfConcreteComponentA() + " + ConcreteVisitor2");
    }

    public void VisitConcreteComponent(ConcreteComponentB element)
    {
        Console.WriteLine(element.SpecialMethodOfConcreteComponentB() + " + ConcreteVisitor2");
    }
}

public class Client
{
    // The client code can run visitor operations over any set of elements
    // without figuring out their concrete classes. The accept operation
    // directs a call to the appropriate operation in the visitor object.
    public static void ClientCode(List<IComponent> components, IVisitor visitor)
    {

    }
}

class Program
{
    static void Main(string[] args)
    {
        List<IComponent> components = new List<IComponent>
        {
            new ConcreteComponentA(),
            new ConcreteComponentB()
        };

        Console.WriteLine("The client code works with all visitors via the base Visitor interface:");
        var visitor1 = new ConcreteVisitor1();
        Client.ClientCode(components,visitor1);

        Console.WriteLine();

        Console.WriteLine("It allows the same client code to work with different types of visitors:");
        var visitor2 = new ConcreteVisitor2();
        Client.ClientCode(components, visitor2);
    }
}
```

##### Intrusive Visitor
```cs
public abstract class Expression {
	public abstract void Print(StringBuilder sb);
}
public class DoubleExpression : Expression
{
	 private double value;
	 public DoubleExpression(double value) { this.value = value; }
}
public class AdditionExpression : Expression
{
	 private Expression left, right;
	 public AdditionExpression(Expression left, Expression right)
	 {
		 this.left = left;
		 this.right = right;
	 }

	 public override void Print(StringBuilder sb)
	 {
		 sb.Append(value: "(");
		 left.Print(sb);
		 sb.Append(value: "+");
		 right.Print(sb);
		 sb.Append(value: ")");
	 }
}

var e = new AdditionExpression(
 new DoubleExpression(1),
 new AdditionExpression(
	 new DoubleExpression(2),
	 new DoubleExpression(3)));
var sb = new StringBuilder();
e.Print(sb);
WriteLine(sb);
```

##### Functional Reflective Visitor
```cs
var e = new AdditionExpression(
 left: new DoubleExpression(1),
 right: new AdditionExpression(
	 left: new DoubleExpression(2),
	 right: new DoubleExpression(3)));
var sb = new StringBuilder();
ExpressionPrinter.Print(e, sb);
WriteLine(sb);

public abstract class Expression {
	public abstract void Print(StringBuilder sb);

	// Functional Reflective Visitor
	public void Match(
	 Action<DoubleExpression> visitDoubleExpression,
	 Action<AdditionExpression> visitAdditionExpression,
	 Action<Expression> visitUnknownExpression = null)
	{
	 switch (this)
	 {
		 case DoubleExpression e:
			 visitDoubleExpression(e);
			 break;
		 case AdditionExpression e:
			 visitAdditionExpression(e);
			 break;
		 default:
			 visitUnknownExpression?.Invoke(this);
			 break;
	 }
	}
}


public class ExpressionPrinter
{
	 private readonly StringBuilder sb = new();
	 public string Print(Expression e)
	 {
		 e.Match(VisitDoubleExpression, VisitAdditionExpression);
		 return sb.ToString();
	 }
	 private void VisitAdditionExpression(AdditionExpression ae) { ... }
	 private void VisitDoubleExpression(DoubleExpression de) { ... }
}
```

##### Classic Visitor
```cs
public abstract class Expression
{
 public abstract void Accept(IExpressionVisitor visitor);
}

// every single implementor of Expression is now required to implement Accept()
public override void Accept(IExpressionVisitor visitor)
{
 visitor.Visit(this);
}


public interface IExpressionVisitor
{
	 void Visit(DoubleExpression de);
	 void Visit(AdditionExpression ae);
}

public class ExpressionPrinter : IExpressionVisitor
{
	 StringBuilder sb = new();
	 public void Visit(DoubleExpression de)
	 {
		 sb.Append(de.Value);
	 }
	 public void Visit(AdditionExpression ae)
	 {
		 sb.Append("(");
		 ae.Left.Accept(this);
		 sb.Append("+");
		 ae.Right.Accept(this);
		 sb.Append(")");
	 }
	 public override string ToString() => sb.ToString();
}

var e = new AdditionExpression(
 new DoubleExpression(1),
 new AdditionExpression(
	 new DoubleExpression(2),
	 new DoubleExpression(3)));
var ep = new ExpressionPrinter();
ep.Visit(e);
WriteLine(ep.ToString()); // (1 + (2 + 3))
```

#### Null Object

##### Nullable Virtual Proxy
This approach is probably the least intrusive and most hygienic, allowing a null
where such a value was hitherto not allowed while, at the same time, using the name of
the default value to hint at what’s going on.
```cs
class OptionalLog : ILog
{
	private ILog impl;
	public OptionalLog(ILog impl) { this.impl = impl; }
	public void Info(string msg) { impl?.Info(msg); }
	public void Warn(string msg) { impl?.Warn(msg); }
}

private const ILog NoLogging = null;
public BankAccount([CanBeNull] ILog log = NoLogging)
{
	this.log = new OptionalLog(log);
}
```

##### Null Object
```cs
 public BankAccount(ILog log)
 {
	 this.log = log;
 }

public sealed class NullLog : ILog
{
	public void Info(string msg) { }
	public void Warn(string msg) { }
}
```

##### Null Object Singleton
```cs
interface ILog
{
	void Info(string msg);
	void Warn(string msg);
	public static ILog Null => NullLog.Instance;
	private sealed class NullLog : ILog
	{
		private NullLog() {}
		private static readonly Lazy<NullLog> instance = new ();
		public static ILog Instance => instance.Value;
		public void Info(string msg) { }
		public void Warn(string msg) { }
	}
}
```
