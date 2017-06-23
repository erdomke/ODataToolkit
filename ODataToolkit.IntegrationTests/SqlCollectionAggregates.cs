namespace ODataToolkit.IntegrationTests.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using ODataToolkit.Tests;

    using ODataToolkit;

    using Machine.Specifications;

    public abstract class SqlCollectionAggregates
    {
        protected static TestDbContext testDb;

        protected static List<ComplexClassDto> result;

        protected static List<NullableClassDto> nullableResult;

        protected static IQueryable<ComplexClassDto> collection;

        protected static IQueryable<NullableClassDto> nullableCollection;

        private Establish context = () =>
            {
                testDb = new TestDbContext();
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<TestDbContext>());

                testDb.Database.ExecuteSqlCommand("UPDATE ComplexClasses SET Concrete_Id = NULL");
                testDb.Database.ExecuteSqlCommand("DELETE FROM NullableValues");
                testDb.Database.ExecuteSqlCommand("DELETE FROM NullableContainers");
                testDb.Database.ExecuteSqlCommand("DELETE FROM EdgeCaseClasses");
                testDb.Database.ExecuteSqlCommand("DELETE FROM ConcreteClasses");
                testDb.Database.ExecuteSqlCommand("DELETE FROM ComplexClasses");

                testDb.ComplexCollection.Add(
                    new ComplexClass
                        {
                            Title = "Charles",
                            Concrete = new ConcreteClass { Children = new List<EdgeCaseClass> { new EdgeCaseClass { Name = "Apple" } } },
                            ConcreteCollection =
                                new List<ConcreteClass>
                                    {
                                        InstanceBuilders.BuildConcrete(
                                            "Apple", 1, new DateTime(2005, 01, 01), true),
                                    }
                        });

                testDb.ComplexCollection.Add(
                    new ComplexClass
                        {
                            Title = "Andrew",
                            Concrete = new ConcreteClass { Children = new List<EdgeCaseClass> { new EdgeCaseClass { Name = "Apple" }, new EdgeCaseClass { Name = "Banana" } } },
                            ConcreteCollection =
                                new List<ConcreteClass>
                                    {
                                        InstanceBuilders.BuildConcrete(
                                            "Apple", 1, new DateTime(2005, 01, 01), true),
                                        InstanceBuilders.BuildConcrete(
                                            "Banana",
                                            2,
                                            new DateTime(2003, 01, 01),
                                            false)
                                    }
                        });

                testDb.ComplexCollection.Add(
                    new ComplexClass
                        {
                            Title = "David",
                            Concrete = new ConcreteClass { Children = new List<EdgeCaseClass> { new EdgeCaseClass { Name = "Apple" }, new EdgeCaseClass { Name = "Banana" }, new EdgeCaseClass { Name = "Custard" } } },
                            ConcreteCollection =
                                new List<ConcreteClass>
                                    {
                                        InstanceBuilders.BuildConcrete(
                                            "Apple", 1, new DateTime(2005, 01, 01), true),
                                        InstanceBuilders.BuildConcrete(
                                            "Banana",
                                            2,
                                            new DateTime(2003, 01, 01),
                                            false),
                                        InstanceBuilders.BuildConcrete(
                                            "Custard",
                                            3,
                                            new DateTime(2007, 01, 01),
                                            true)
                                    }
                        });

                testDb.ComplexCollection.Add(
                    new ComplexClass
                        {
                            Title = "Edward",
                            Concrete = new ConcreteClass { Children = new List<EdgeCaseClass> { new EdgeCaseClass { Name = "Apple" }, new EdgeCaseClass { Name = "Custard" }, new EdgeCaseClass { Name = "Dogfood" }, new EdgeCaseClass { Name = "Eggs" } } },
                            ConcreteCollection =
                                new List<ConcreteClass>
                                    {
                                        InstanceBuilders.BuildConcrete(
                                            "Apple", 1, new DateTime(2005, 01, 01), true),
                                        InstanceBuilders.BuildConcrete(
                                            "Custard",
                                            3,
                                            new DateTime(2007, 01, 01),
                                            true),
                                        InstanceBuilders.BuildConcrete(
                                            "Dogfood",
                                            4,
                                            new DateTime(2009, 01, 01),
                                            false),
                                        InstanceBuilders.BuildConcrete(
                                            "Eggs", 5, new DateTime(2000, 01, 01), true)
                                    }
                        });

                testDb.ComplexCollection.Add(
                    new ComplexClass
                        {
                            Title = "Boris",
                            Concrete = new ConcreteClass { Children = new List<EdgeCaseClass> { new EdgeCaseClass { Name = "Apple" }, new EdgeCaseClass { Name = "Dogfood" }, new EdgeCaseClass { Name = "Eggs" } } },
                            ConcreteCollection =
                                new List<ConcreteClass>
                                    {
                                        InstanceBuilders.BuildConcrete(
                                            "Apple", 1, new DateTime(2005, 01, 01), true),
                                        InstanceBuilders.BuildConcrete(
                                            "Dogfood",
                                            4,
                                            new DateTime(2009, 01, 01),
                                            false),
                                        InstanceBuilders.BuildConcrete(
                                            "Eggs", 5, new DateTime(2000, 01, 01), true)
                                    }
                        });

                testDb.SaveChanges();

                testDb.NullableContainers.Add(new NullableContainer { Nullables = new List<NullableValue> { new NullableValue { Age = null } } });
                testDb.NullableContainers.Add(new NullableContainer { Nullables = new List<NullableValue> { new NullableValue { Age = 1 } } });
                testDb.NullableContainers.Add(new NullableContainer { Nullables = new List<NullableValue> { new NullableValue { Age = 1 }, new NullableValue { Age = 2 } } });
                testDb.NullableContainers.Add(new NullableContainer { Nullables = new List<NullableValue> { new NullableValue { Age = null }, new NullableValue { Age = 1 }, new NullableValue { Age = 2 } } });

                testDb.SaveChanges();

                collection =
                    testDb.ComplexCollection.Select(
                        o =>
                        new ComplexClassDto
                            {
                                Title = o.Title,
                                StringCollection = o.ConcreteCollection.Select(c => c.Name),
                                IntCollection = o.ConcreteCollection.Select(c => c.Age),
                                ConcreteCollection = o.ConcreteCollection,
                                Concrete = o.Concrete
                            });

                nullableCollection = testDb.NullableContainers.Select(
                    o => new NullableClassDto
                         {
                             NullableCollection = o.Nullables.Select(n => n.Age)
                         });

                testDb = new TestDbContext();
            };
    }

    #region Nullable Int Collections

    public class When_filtering_on_a_nullable_int_collection_property_using_any_checking_for_nulls : SqlCollectionAggregates
    {
        private Because of = () => nullableResult = nullableCollection.ExecuteOData("$filter=NullableCollection/any(int: int eq null)").ToList();

        private It should_return_two_records = () => nullableResult.Count().ShouldEqual(2);

        private It should_only_return_records_where_string_collection_contains_banana = () => nullableResult.ShouldEachConformTo(o => o.NullableCollection.Any(s => s == null));
    }

    #endregion

    #region String collections

    public class When_filtering_on_a_simple_collection_property_using_any : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/any(tag: tag eq 'Banana')").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_string_collection_contains_banana = () => result.ShouldEachConformTo(o => o.StringCollection.Any(s => s == "Banana"));
    }

    public class When_filtering_on_a_simple_collection_property_using_any_with_an_or : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/any(tag: tag eq 'Banana' or tag eq 'Eggs')").ToList();

        private It should_return_four_records = () => result.Count().ShouldEqual(4);

        private It should_only_return_records_where_string_collection_contains_banana_or_eggs = () => result.ShouldEachConformTo(o => o.StringCollection.Any(s => s == "Banana" || s == "Eggs"));
    }

    public class When_filtering_on_a_simple_collection_property_using_any_with_functions : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/any(tag: startswith(tag,'Dog'))").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_string_collection_contains_value_starting_with_dog = () => result.ShouldEachConformTo(o => o.StringCollection.Any(s => s.StartsWith("Dog")));
    }

    public class When_filtering_on_a_simple_collection_property_using_all : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/all(tag: tag eq 'Apple')").ToList();

        private It should_return_one_records = () => result.Count().ShouldEqual(1);

        private It should_only_return_records_where_all_string_collection_records_are_apple = () => result.ShouldEachConformTo(o => o.StringCollection.All(s => s == "Apple"));
    }

    public class When_filtering_on_a_simple_collection_property_using_all_with_an_or : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/all(tag: tag eq 'Apple' or tag eq 'Banana')").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_all_string_collection_records_are_apple_or_banana = () => result.ShouldEachConformTo(o => o.StringCollection.All(s => s == "Apple" || s == "Banana"));
    }

    public class When_filtering_on_a_simple_collection_property_using_all_with_functions : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/all(tag: startswith(tag,'App'))").ToList();

        private It should_return_one_record = () => result.Count().ShouldEqual(1);

        private It should_only_return_records_where_all_string_collection_records_start_with_app = () => result.ShouldEachConformTo(o => o.StringCollection.All(s => s.StartsWith("App")));
    }

    public class When_filtering_on_a_simple_collection_property_using_count : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=StringCollection/$count ge 3").ToList();

        private It should_return_three_records = () => result.Count().ShouldEqual(3);

        private It should_only_return_records_where_string_collection_count_is_greater_than_or_equal_to_3 = () => result.ShouldEachConformTo(o => o.StringCollection.Count() >= 3);
    }

    #endregion

    #region Complex collections

    public class When_filtering_on_a_complex_collection_property_using_any : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/any(concrete: concrete/Name eq 'Banana')").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_concrete_collection_contains_value_with_name_banana = () => result.ShouldEachConformTo(o => o.ConcreteCollection.Any(s => s.Name == "Banana"));
    }

    public class When_filtering_on_a_complex_collection_property_using_any_with_an_or : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/any(concrete: concrete/Name eq 'Banana' or concrete/Name eq 'Eggs')").ToList();

        private It should_return_four_records = () => result.Count().ShouldEqual(4);

        private It should_only_return_records_where_concrete_collection_contains_value_with_name_banana_or_eggs = () => result.ShouldEachConformTo(o => o.ConcreteCollection.Any(s => s.Name == "Banana" || s.Name == "Eggs"));
    }

    public class When_filtering_on_a_complex_collection_property_using_any_with_functions : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/any(concrete: startswith(concrete/Name,'Dog'))").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_concrete_collection_contains_value_with_name_starting_with_dog = () => result.ShouldEachConformTo(o => o.ConcreteCollection.Any(s => s.Name.StartsWith("Dog")));
    }

    public class When_filtering_on_a_complex_collection_property_using_all : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/all(concrete: concrete/Name eq 'Apple')").ToList();

        private It should_return_one_records = () => result.Count().ShouldEqual(1);

        private It should_only_return_records_where_all_concrete_collection_values_have_name_apple = () => result.ShouldEachConformTo(o => o.ConcreteCollection.All(s => s.Name == "Apple"));
    }

    public class When_filtering_on_a_complex_collection_property_using_all_with_an_or : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/all(concrete: concrete/Name eq 'Apple' or concrete/Name eq 'Banana')").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_all_concrete_collection_values_have_name_apple_or_banana = () => result.ShouldEachConformTo(o => o.ConcreteCollection.All(s => s.Name == "Apple" || s.Name == "Banana"));
    }

    public class When_filtering_on_a_complex_collection_property_using_all_with_functions : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/all(concrete: startswith(concrete/Name,'App'))").ToList();

        private It should_return_one_record = () => result.Count().ShouldEqual(1);

        private It should_only_return_records_where_all_concrete_collection_values_have_name_starting_with_app = () => result.ShouldEachConformTo(o => o.ConcreteCollection.All(s => s.Name.StartsWith("App")));
    }

    public class When_filtering_on_a_complex_collection_property_using_count : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=ConcreteCollection/$count ge 3").ToList();

        private It should_return_three_records = () => result.Count().ShouldEqual(3);

        private It should_only_return_records_where_concrete_collection_count_is_greater_than_3 = () => result.ShouldEachConformTo(o => o.ConcreteCollection.Count() >= 3);
    }

    #endregion

    #region Nested Complex collections

    public class When_filtering_on_a_nested_simple_collection_property_using_any : SqlCollectionAggregates
    {
        private Because of = () => result = collection.ExecuteOData("$filter=Concrete/Children/any(child: child/Name eq 'Banana')").ToList();

        private It should_return_two_records = () => result.Count().ShouldEqual(2);

        private It should_only_return_records_where_concrete_contains_child_with_name_banana = () => result.ShouldEachConformTo(o => o.Concrete.Children.Any(s => s.Name == "Banana"));
    }

    #endregion
}
