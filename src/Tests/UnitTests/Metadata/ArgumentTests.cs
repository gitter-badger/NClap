﻿using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentTests
    {
        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class StringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = true, ShortName = "v", DefaultValue = "def", Description = "Some value")]
            public string Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class StringArgumentsThatMustBeNonEmpty
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = false, DefaultValue = "")]
            [MustNotBeEmpty]
            public string Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class RestOfLineStringArguments
        {
            [PositionalArgument(ArgumentFlags.RestOfLine)]
            public string Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class KeyValuePairArguments
        {
            [NamedArgument(ArgumentFlags.Required, ShortName = "w")]
            public KeyValuePair<int, int> Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class StringArrayArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, AllowEmpty = true, DefaultValue = new[] { "a", "b" })]
            public string[] Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class StringArrayWithEmptyDefaultArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, AllowEmpty = true, DefaultValue = new string[] {})]
            public string[] Value;
        }

        public enum TestEnum
        {
            First,
            Second,
            Third
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class EnumArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public TestEnum Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class BoolArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public bool Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class BoolArgumentsWithTrueDefault
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = true)]
            public bool Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class EmptyLongNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "")]
            public string Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class TupleOfIntAndStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public Tuple<int, string> Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class ArgumentsWithUnsettableDefault
        {
            public bool underlyingValue;

            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = true)]
            public bool Value
            {
                get => underlyingValue;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class ArgumentsWithUnsettableCollectionDefault
        {
            public string[] underlyingValue;

            [NamedArgument(ArgumentFlags.Multiple, DefaultValue = new string[] { })]
            public string[] Value
            {
                get => underlyingValue;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class DefaultedNamedArguments
        {
            [NamedArgument]
            public string ValueArgument;
        }

        [TestMethod]
        public void InvalidNamedArgumentPrefixes()
        {
            var arg = GetArgument(typeof(StringArguments), setAttrib: new ArgumentSetAttribute
            {
                NamedArgumentPrefixes = new string[] { },
            });

            Action getSyntaxHelpWithBogusSeparators = () => arg.GetSyntaxSummary();
            getSyntaxHelpWithBogusSeparators.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void InvalidArgumentValueSeparators()
        {
            var arg = GetArgument(typeof(StringArguments), setAttrib: new ArgumentSetAttribute
            {
                ArgumentValueSeparators = new char[] { }
            });

            Action getSyntaxHelpWithBogusSeparators = () => arg.GetSyntaxSummary();
            getSyntaxHelpWithBogusSeparators.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void StringArgument()
        {
            var arg = GetArgument(typeof(StringArguments));
            arg.EffectiveDefaultValue.Should().Be("def");
            arg.DefaultValue.Should().Be("def");
            arg.GetSyntaxSummary().Should().Be("[/Value[=<String>]]");
        }

        [TestMethod]
        public void NonEmptyStringArgument()
        {
            var arg = GetArgument(typeof(StringArgumentsThatMustBeNonEmpty));
            arg.EffectiveDefaultValue.Should().Be("");
            arg.DefaultValue.Should().Be("");
            arg.GetSyntaxSummary().Should().Be("[/Value=<String>]");
           
            var usageInfo = new ArgumentUsageInfo(arg);
            usageInfo.DefaultValue.Should().BeNull();
        }

        [TestMethod]
        public void RestOfLineArgument()
        {
            var arg = GetArgument(typeof(RestOfLineStringArguments));
            arg.EffectiveDefaultValue.Should().BeNull();
            arg.GetSyntaxSummary().Should().Be("[<Value> : <String>...]");
        }

        [TestMethod]
        public void KeyValuePairArgument()
        {
            var arg = GetArgument(typeof(KeyValuePairArguments));
            arg.EffectiveDefaultValue.Should().Be(new KeyValuePair<int, int>(0, 0));
            arg.GetSyntaxSummary().Should().Be("/Value=<Int32>=<Int32>");
        }

        [TestMethod]
        public void StringArrayArgument()
        {
            var arg = GetArgument(typeof(StringArrayArguments));

            var value = arg.EffectiveDefaultValue;
            value.Should().BeOfType(typeof(string[]));
            ((string[])value).Should().ContainInOrder("a", "b");

            arg.GetSyntaxSummary().Should().Be("[/Value[=<String>]]*");

            var usage = new ArgumentUsageInfo(arg);
            usage.DefaultValue.Should().Be("a b");
        }

        [TestMethod]
        public void StringArrayWithEmptyDefaultsArgument()
        {
            var arg = GetArgument(typeof(StringArrayWithEmptyDefaultArguments));

            var value = arg.EffectiveDefaultValue;
            value.Should().BeOfType(typeof(string[]));
            ((string[])value).Should().BeEmpty();

            arg.GetSyntaxSummary().Should().Be("[/Value[=<String>]]*");

            var usage = new ArgumentUsageInfo(arg);
            usage.DefaultValue.Should().BeNull();
        }

        [TestMethod]
        public void EnumArgument()
        {
            var arg = GetArgument(typeof(EnumArguments));
            arg.EffectiveDefaultValue.Should().Be(TestEnum.First);
            arg.GetSyntaxSummary().Should().Be("[/Value=<TestEnum>]");
        }

        [TestMethod]
        public void BoolArgument()
        {
            var arg = GetArgument(typeof(BoolArguments));
            arg.EffectiveDefaultValue.Should().Be(false);
            arg.GetSyntaxSummary().Should().Be("[/Value]");
        }

        [TestMethod]
        public void BoolArgumentWithTrueDefault()
        {
            var arg = GetArgument(typeof(BoolArgumentsWithTrueDefault));
            arg.EffectiveDefaultValue.Should().Be(true);
            arg.GetSyntaxSummary().Should().Be("[/Value[=<bool>]]");
        }

        [TestMethod]
        public void EmptyLongNameArgument()
        {
            Action getArg = () => GetArgument(typeof(EmptyLongNameArguments));
            getArg.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void TupleOfIntAndStringArgument()
        {
            var arg = GetArgument(typeof(TupleOfIntAndStringArguments));
            arg.EffectiveDefaultValue.Should().BeNull();
            arg.GetSyntaxSummary().Should().Be("[/Value=<Int32>,<String>]");
        }

        [TestMethod]
        public void ArgumentWithUnsettableDefault()
        {
            var arg = GetArgument(typeof(ArgumentsWithUnsettableDefault));
            arg.DefaultValue.Should().Be(true);
            arg.EffectiveDefaultValue.Should().Be(true);
            arg.TryFinalize(new ArgumentsWithUnsettableDefault(), FileSystemReader.Create()).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentWithUnsettableCollectionDefault()
        {
            var arg = GetArgument(typeof(ArgumentsWithUnsettableCollectionDefault));
            arg.DefaultValue.Should().BeOfType<string[]>();
            arg.TryFinalize(new ArgumentsWithUnsettableCollectionDefault(), FileSystemReader.Create()).Should().BeFalse();
        }

        [TestMethod]
        public void PreservedCaseShortNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument");
            arg.ExplicitShortName.Should().BeFalse();
            arg.ShortName.Should().Be("V");
        }

        [TestMethod]
        public void LowercaseShortNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument", setAttrib: new ArgumentSetAttribute
            {
                NameGenerationFlags = ArgumentNameGenerationFlags.PreferLowerCaseForShortNames
            });

            arg.ExplicitShortName.Should().BeFalse();
            arg.ShortName.Should().Be("v");
        }

        [TestMethod]
        public void PreservedCaseLongNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument");
            arg.LongName.Should().Be("ValueArgument");
        }

        [TestMethod]
        public void LowercaseAndHyphenatedLongNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument", setAttrib: new ArgumentSetAttribute
            {
                NameGenerationFlags = ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames
            });
            arg.LongName.Should().Be("value-argument");
        }

        internal static ArgumentDefinition GetArgument(Type type, string fieldName = "Value", ArgumentSetAttribute setAttrib = null)
        {
            var argMember = type.GetMember(fieldName)[0];
            var attrib = argMember.GetSingleAttribute<ArgumentBaseAttribute>();
            attrib.Should().NotBeNull();

            var options = new CommandLineParserOptions
            {
                FileSystemReader = FileSystemReader.Create(),
                Reporter = err => { }
            };

            IMutableMemberInfo mutableMemberInfo;
            switch (argMember)
            {
            case FieldInfo fi:
                mutableMemberInfo = new MutableFieldInfo(fi);
                break;
            case PropertyInfo pi:
                mutableMemberInfo = new MutablePropertyInfo(pi);
                break;
            default:
                throw new NotSupportedException();
            }

            return new ArgumentDefinition(
                mutableMemberInfo,
                attrib,
                setAttrib ?? new ArgumentSetAttribute { Style = ArgumentSetStyle.WindowsCommandLine },
                options);
        }
    }
}
