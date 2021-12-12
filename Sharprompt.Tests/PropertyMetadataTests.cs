using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Sharprompt.Forms;
using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests
{
    public class PropertyMetadataTests
    {
        [Fact]
        public void Basic()
        {
            var metadata = PropertyMetadataFactory.Create(new BasicModel());

            Assert.NotNull(metadata);
            Assert.Equal(1, metadata.Count);

            Assert.Equal(typeof(string), metadata[0].Type);
            Assert.Equal(FormType.Input, metadata[0].DetermineFormType());
            Assert.Equal("Input Value", metadata[0].Message);
            Assert.Equal("Required Value", metadata[0].Placeholder);
            Assert.Null(metadata[0].DefaultValue);
            Assert.Null(metadata[0].Order);
            Assert.Equal(1, metadata[0].Validators.Count);
        }

        [Fact]
        public void Basic_DefaultValue()
        {
            var metadata = PropertyMetadataFactory.Create(new BasicModel { Value = "sample" });

            Assert.NotNull(metadata);
            Assert.Equal(1, metadata.Count);

            Assert.Equal(typeof(string), metadata[0].Type);
            Assert.Equal("sample", metadata[0].DefaultValue);
        }

        [Fact]
        public void Complex()
        {
            var metadata = PropertyMetadataFactory.Create(new ComplexModel());

            Assert.NotNull(metadata);
            Assert.Equal(3, metadata.Count);

            Assert.Equal(typeof(string), metadata[0].Type);
            Assert.Equal(FormType.Input, metadata[0].DetermineFormType());

            Assert.Equal(typeof(int), metadata[1].Type);
            Assert.Equal(FormType.Input, metadata[1].DetermineFormType());

            Assert.Equal(typeof(bool), metadata[2].Type);
            Assert.Equal(FormType.Confirm, metadata[2].DetermineFormType());
        }

        [Fact]
        public void Complex_DefaultValue()
        {
            var metadata = PropertyMetadataFactory.Create(new ComplexModel { Value1 = "sample", Value2 = 42, Value3 = true });

            Assert.NotNull(metadata);
            Assert.Equal(3, metadata.Count);

            Assert.Equal(typeof(string), metadata[0].Type);
            Assert.Equal(FormType.Input, metadata[0].DetermineFormType());
            Assert.Equal("sample", metadata[0].DefaultValue);

            Assert.Equal(typeof(int), metadata[1].Type);
            Assert.Equal(FormType.Input, metadata[1].DetermineFormType());
            Assert.Equal(42, metadata[1].DefaultValue);

            Assert.Equal(typeof(bool), metadata[2].Type);
            Assert.Equal(FormType.Confirm, metadata[2].DetermineFormType());
            Assert.Equal(true, metadata[2].DefaultValue);
        }

        [Fact]
        public void Complex_Order()
        {
            var metadata = PropertyMetadataFactory.Create(new ComplexWithOrderModel { Value1 = "sample", Value2 = 42, Value3 = true });

            Assert.NotNull(metadata);
            Assert.Equal(3, metadata.Count);

            Assert.Equal(typeof(int), metadata[0].Type);
            Assert.Equal(FormType.Input, metadata[0].DetermineFormType());
            Assert.Equal(42, metadata[0].DefaultValue);

            Assert.Equal(typeof(bool), metadata[1].Type);
            Assert.Equal(FormType.Confirm, metadata[1].DetermineFormType());
            Assert.Equal(true, metadata[1].DefaultValue);

            Assert.Equal(typeof(string), metadata[2].Type);
            Assert.Equal(FormType.Input, metadata[2].DetermineFormType());
            Assert.Equal("sample", metadata[2].DefaultValue);
        }

        [Fact]
        public void Nullable()
        {
            var metadata = PropertyMetadataFactory.Create(new NullableModel());

            Assert.NotNull(metadata);
            Assert.Equal(3, metadata.Count);

            Assert.Equal(typeof(int?), metadata[0].Type);
            Assert.Equal(FormType.Input, metadata[0].DetermineFormType());

            Assert.Equal(typeof(bool?), metadata[1].Type);
            Assert.Equal(FormType.Input, metadata[1].DetermineFormType());

            Assert.Equal(typeof(double?), metadata[2].Type);
            Assert.Equal(FormType.Input, metadata[2].DetermineFormType());
        }

        [Fact]
        public void Enum()
        {
            var metadata = PropertyMetadataFactory.Create(new EnumModel());

            Assert.NotNull(metadata);
            Assert.Equal(3, metadata.Count);

            Assert.Equal(typeof(EnumValue), metadata[0].Type);
            Assert.Equal(FormType.Select, metadata[0].DetermineFormType());

            Assert.Equal(typeof(EnumValue?), metadata[1].Type);
            Assert.Equal(FormType.Input, metadata[1].DetermineFormType());

            Assert.Equal(typeof(IEnumerable<EnumValue>), metadata[2].Type);
            Assert.Equal(FormType.MultiSelect, metadata[2].DetermineFormType());
        }

        [Fact]
        public void InlineDataSource()
        {
            var metadata = PropertyMetadataFactory.Create(new InlineDataSourceModel());

            Assert.NotNull(metadata);
            Assert.Equal(2, metadata.Count);

            Assert.Equal(FormType.Select, metadata[0].DetermineFormType());
            Assert.Equal(Enumerable.Range(1, 5), metadata[0].ItemsSourceProvider.GetItems<int>());

            Assert.Equal(FormType.MultiSelect, metadata[1].DetermineFormType());
            Assert.Equal(Enumerable.Range(1, 10), metadata[1].ItemsSourceProvider.GetItems<int>());
        }

        [Fact]
        public void MemberDataSource()
        {
            var metadata = PropertyMetadataFactory.Create(new MemberDataSourceModel());

            Assert.NotNull(metadata);
            Assert.Equal(2, metadata.Count);

            Assert.Equal(FormType.Select, metadata[0].DetermineFormType());
            Assert.Equal(Enumerable.Range(1, 5), metadata[0].ItemsSourceProvider.GetItems<int>());

            Assert.Equal(FormType.Select, metadata[1].DetermineFormType());
            Assert.Equal(Enumerable.Range(1, 10), metadata[1].ItemsSourceProvider.GetItems<int>());
        }

        public class BasicModel
        {
            [Display(Name = "Input Value", Prompt = "Required Value")]
            [Required]
            public string Value { get; set; }
        }

        public class ComplexModel
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
            public bool Value3 { get; set; }
        }

        public class ComplexWithOrderModel
        {
            [Display(Order = 3)]
            public string Value1 { get; set; }

            [Display(Order = 1)]
            public int Value2 { get; set; }

            [Display(Order = 2)]
            public bool Value3 { get; set; }
        }

        public class NullableModel
        {
            public int? IntValue { get; set; }
            public bool? BoolValue { get; set; }
            public double? DoubleValue { get; set; }
        }

        public class EnumModel
        {
            public EnumValue Enum1 { get; set; }
            public EnumValue? Enum2 { get; set; }
            public IEnumerable<EnumValue> Enum3 { get; set; }
        }

        public enum EnumValue
        {
            Value1,
            Value2,
            Value3
        }

        public class InlineDataSourceModel
        {
            [InlineItemsSource(1, 2, 3, 4, 5)]
            public int IntValue { get; set; }

            [InlineItemsSource(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
            public IEnumerable<int> IntArray { get; set; }
        }


        public class MemberDataSourceModel
        {
            [MemberItemsSource(typeof(MemberDataSourceModel), nameof(GetSelectItems))]
            public int MemberValue { get; set; }

            [MemberItemsSource(typeof(MemberDataSourceModel), nameof(SelectItems))]
            public int PropertyValue { get; set; }

            public static IEnumerable<int> GetSelectItems()
            {
                return new[] { 1, 2, 3, 4, 5 };
            }

            public static IEnumerable<int> SelectItems => new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        }
    }
}
