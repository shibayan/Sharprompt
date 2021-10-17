using System.ComponentModel.DataAnnotations;

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

        public class BasicModel
        {
            [Display(Prompt = "Input Value")]
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
    }
}
