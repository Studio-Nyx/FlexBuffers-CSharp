using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using FlexBuffers;
using NUnit.Framework;

namespace FlexBuffersCSharpTests
{
    [TestFixture]
    public class FlxValueTests
    {
        [Test]
        public void Null()
        {
            var bytes = FlexBuffer.Null();
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(true, flx.IsNull);
        }
        
        [Test]
        public void Long()
        {
            CheckLong(0);
            CheckLong(1);
            CheckLong(-5);
            CheckLong(byte.MaxValue);
            CheckLong(byte.MinValue);
            CheckLong(short.MaxValue);
            CheckLong(short.MinValue);
            CheckLong(int.MaxValue);
            CheckLong(int.MinValue);
            CheckLong(long.MaxValue);
            CheckLong(long.MinValue);
        }
        
        [Test]
        public void ULong()
        {
            CheckULong(byte.MaxValue);
            CheckULong(byte.MinValue);
            CheckULong(ushort.MaxValue);
            CheckULong(ushort.MinValue);
            CheckULong(uint.MaxValue);
            CheckULong(uint.MinValue);
            CheckULong(ulong.MaxValue);
            CheckULong(ulong.MinValue);
            CheckLongAsULong(1);
            CheckLongAsULong(sbyte.MaxValue);
            CheckLongAsULong(short.MaxValue);
            CheckLongAsULong(int.MaxValue);
            CheckLongAsULong(long.MaxValue);
        }
        
        [Test]
        public void Double()
        {
            CheckDouble(0);
            CheckDouble(-5);
            CheckDouble(-5.1);
            CheckDouble(0.1);
            CheckDouble(5.5);
            CheckDouble(5.25);
            CheckLongAsDouble(long.MaxValue);
            CheckLongAsDouble(long.MinValue);
            CheckULongAsDouble(ulong.MaxValue);
        }
        
        [Test]
        public void Bool()
        {
            CheckBool(true);
            CheckBool(false);
            CheckLongAsBool(0);
            CheckLongAsBool(1);
            CheckLongAsBool(-1);
            CheckULongAsBool(1);
            CheckULongAsBool(5);
            CheckULongAsBool(0);
        }
        
        [Test]
        public void String()
        {
            CheckString("");
            CheckString("Maxim");
            CheckString("Max");
            CheckString("Alex");
            CheckString("Hi 😍 🤓 🥳");
        }
        
        [Test]
        public void Blob()
        {
            var value = new byte[] {1, 2, 3, 4};
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsBlob);
        }
        
        [Test]
        public void BlobToJson()
        {
            var value = new byte[] {1, 2, 3, 4};
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual("\"AQIDBA==\"", flx.ToJson);
            Assert.AreEqual(value, Convert.FromBase64String("AQIDBA=="));
        }
        
        [Test]
        public void IntArray()
        {
            var value = new[] {1, 2, 3, -1};
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Length, flx.AsVector.Length);
            for (var i = 0; i < value.Length; i++)
            {
                Assert.AreEqual(value[i], flx[i].AsLong, $"Not equal at index {i}");
            }
        }
        
        [Test]
        public void DoubleArray()
        {
            var value = new[] {1.1, 2.5, 3, -1};
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Length, flx.AsVector.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var v = flx[i].AsDouble;
                Assert.AreEqual(value[i], v, $"Not equal at index {i}");
            }
        }
        
        [Test]
        public void BoolArray()
        {
            var value = new[] {true, false, true, true};
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Length, flx.AsVector.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var v = flx[i].AsBool;
                Assert.AreEqual(value[i], v, $"Not equal at index {i}");
            }
        }
        
        [Test]
        public void StringArray()
        {
            var value = new[] {"Max", "Maxim", "Alex", "Hi 😂🤣😍🤪"};
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Length, flx.AsVector.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var v = flx[i].AsString;
                Assert.AreEqual(value[i], v, $"Not equal at index {i}");
            }
        }
        
        [Test]
        public void MixedArray()
        {
            var value = new object[] {"Max", 1, 0.1, 0.5, true, int.MinValue, ulong.MaxValue, "Hi 😂🤣😍🤪", null};
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Length, flx.AsVector.Length);
            Assert.AreEqual(value[0], flx[0].AsString);
            Assert.AreEqual(value[1], flx[1].AsLong);
            Assert.AreEqual(value[2], flx[2].AsDouble);
            Assert.AreEqual(value[3], flx[3].AsDouble);
            Assert.AreEqual(value[4], flx[4].AsBool);
            Assert.AreEqual(value[5], flx[5].AsLong);
            Assert.AreEqual(value[6], flx[6].AsULong);
            Assert.AreEqual(value[7], flx[7].AsString);
            Assert.AreEqual(true, flx[8].IsNull);
        }
        
        [Test]
        public void SimpleMap()
        {
            var value = new Dictionary<string, int>()
            {
                {"a", 12},
                {"b", 45}
            };
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Count, flx.AsMap.Length);
            Assert.AreEqual(value["a"], flx["a"].AsLong);
            Assert.AreEqual(value["b"], flx["b"].AsLong);
        }
        
        [Test]
        public void ComplexMap()
        {
            var value = new Dictionary<string, dynamic>()
            {
                {"age", 35},
                {"flags", new[]{true, false, true, true}},
                {"weight", 72.5},
                {"name", "Maxim"},
                {"address", new Dictionary<string, dynamic>()
                {
                    {"city", "Bla"},
                    {"zip", "12345"},
                    {"countryCode", "XX"},
                }}, 
            };
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value.Count, flx.AsMap.Length);
            
            Assert.AreEqual(35, flx["age"].AsLong);
            Assert.AreEqual(72.5, flx["weight"].AsDouble);
            Assert.AreEqual("Maxim", flx["name"].AsString);
            
            Assert.AreEqual(4, flx["flags"].AsVector.Length);
            Assert.AreEqual(true, flx["flags"][0].AsBool);
            Assert.AreEqual(false, flx["flags"][1].AsBool);
            Assert.AreEqual(true, flx["flags"][2].AsBool);
            Assert.AreEqual(true, flx["flags"][3].AsBool);
            
            Assert.AreEqual(3, flx["address"].AsMap.Length);
            Assert.AreEqual("Bla", flx["address"]["city"].AsString);
            Assert.AreEqual("12345", flx["address"]["zip"].AsString);
            Assert.AreEqual("XX", flx["address"]["countryCode"].AsString);
        }
        
        [Test]
        public void ComplexMapToJson()
        {
            var value = new Dictionary<string, dynamic>()
            {
                {"age", 35},
                {"flags", new[]{true, false, true, true}},
                {"weight", 72.5},
                {"name", "Maxim"},
                {"address", new Dictionary<string, dynamic>()
                {
                    {"city", "Bla"},
                    {"zip", "12345"},
                    {"countryCode", "XX"},
                }},
                {"something", null}
            };
            var bytes = FlexBuffer.From(value);
            var flx = FlxValue.FromBytes(bytes);
            const string expected = "{\"address\":{\"city\":\"Bla\",\"countryCode\":\"XX\",\"zip\":\"12345\"},\"age\":35,\"flags\":[true,false,true,true],\"name\":\"Maxim\",\"something\":null,\"weight\":72.5}";
            var json = flx.ToJson;
            Assert.AreEqual(expected, json);
        }
        
        [Test]
        public void Long2()
        {
            var bytes = FlexBuffer.SingleValue(1, 2);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsLong, 1);
            Assert.AreEqual(flx[1].AsLong, 2);
            
            bytes = FlexBuffer.SingleValue(1, 256);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsLong, 1);
            Assert.AreEqual(flx[1].AsLong, 256);
            
            bytes = FlexBuffer.SingleValue(1, long.MaxValue);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsLong, 1);
            Assert.AreEqual(flx[1].AsLong, long.MaxValue);
        }
        
        [Test]
        public void ULong2()
        {
            var bytes = FlexBuffer.SingleValue(1UL, 2);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsULong, 1);
            Assert.AreEqual(flx[1].AsULong, 2);
            
            bytes = FlexBuffer.SingleValue(1, 256);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsULong, 1);
            Assert.AreEqual(flx[1].AsULong, 256);
            
            bytes = FlexBuffer.SingleValue(1, ulong.MaxValue);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsULong, 1);
            Assert.AreEqual(flx[1].AsULong, ulong.MaxValue);
        }
        
        [Test]
        public void Double2()
        {
            var bytes = FlexBuffer.SingleValue(1.1f, 2);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsDouble, 1.1f);
            Assert.AreEqual(flx[1].AsDouble, 2);
            
            bytes = FlexBuffer.SingleValue(1.1, 256);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(2, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsDouble, 1.1);
            Assert.AreEqual(flx[1].AsDouble, 256);
        }
        
        [Test]
        public void Double3()
        {
            var bytes = FlexBuffer.SingleValue(1.1f, 2, 3.3f);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(3, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsDouble, 1.1f);
            Assert.AreEqual(flx[1].AsDouble, 2);
            Assert.AreEqual(flx[2].AsDouble, 3.3f);
            
            bytes = FlexBuffer.SingleValue(1.1, 256, 3.3);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(3, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsDouble, 1.1);
            Assert.AreEqual(flx[1].AsDouble, 256);
            Assert.AreEqual(flx[2].AsDouble, 3.3);
        }
        
        [Test]
        public void Double4()
        {
            var bytes = FlexBuffer.SingleValue(1.1f, 2, 3.3f, 0.5);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(4, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsDouble, 1.1f);
            Assert.AreEqual(flx[1].AsDouble, 2);
            Assert.AreEqual(flx[2].AsDouble, 3.3f);
            Assert.AreEqual(flx[3].AsDouble, 0.5);
            
            bytes = FlexBuffer.SingleValue(1.1, 256, 3.3, 0.5);
            flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(4, flx.AsVector.Length);
            Assert.AreEqual(flx[0].AsDouble, 1.1);
            Assert.AreEqual(flx[1].AsDouble, 256);
            Assert.AreEqual(flx[2].AsDouble, 3.3);
            Assert.AreEqual(flx[3].AsDouble, 0.5);
        }

        [Test]
        public void IterateOverVector()
        {
            var ints = new[] {1, 2, 3};
            var bytes = FlexBuffer.From(ints);
            var flx = FlxValue.FromBytes(bytes);
            var i = 0;
            foreach (var value in flx.AsVector)
            {
                Assert.AreEqual(ints[i], value.AsLong);
                i++;
            }
            Assert.AreEqual(3, i);
        }
        
        [Test]
        public void IterateOverMap()
        {
            var dict = new Dictionary<string, int>()
            {
                {"a", 1},
                {"b", 2},
                {"c", 3},
            };
            var bytes = FlexBuffer.From(dict);
            var flx = FlxValue.FromBytes(bytes);
            var i = 0;
            foreach (var value in flx.AsMap)
            {
                Assert.AreEqual(dict[value.Key], value.Value.AsLong);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        [Test]
        public void GetValueIndex()
        {
            var dict = new Dictionary<string, int>()
            {
                {"a", 1},
                {"b", 2},
                {"c", 3},
            };
            var bytes = FlexBuffer.From(dict);
            var flx = FlxValue.FromBytes(bytes);
            var map = flx.AsMap;
            Assert.AreEqual(3, map.Length);
            Assert.AreEqual(0, map.KeyIndex("a"));
            Assert.AreEqual(1, map.KeyIndex("b"));
            Assert.AreEqual(2, map.KeyIndex("c"));
            Assert.AreEqual(-1, map.KeyIndex("d"));
            Assert.AreEqual(1, map.ValueByIndex(0).AsLong);
            
        }

        [Test]
        public void TestPrettyJson2DVector()
        {
            var list = new List<List<int>>
            {
                new List<int>(){1, 2, 3},
                new List<int>(){5, 6},
                new List<int>(){7, 8, 9, 10},
            };

            var expected = @"[
  [
    1,
    2,
    3
  ],
  [
    5,
    6
  ],
  [
    7,
    8,
    9,
    10
  ]
]".Replace("\r", "");
            var bytes = FlexBuffer.From(list);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(expected, flx.ToPrettyJson());
        } 
        
        [Test]
        public void TestPrettyJsonMap()
        {
            var dict = new Dictionary<string, object>()
            {
                {"a", 1},
                {"b", 2},
                {"c", 3},
                {"d", new []{1, 2, 3}}
            };

            var expected = @"{
  ""a"" : 1,
  ""b"" : 2,
  ""c"" : 3,
  ""d"" : [
    1,
    2,
    3
  ]
}".Replace("\r", "");
            var bytes = FlexBuffer.From(dict);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(expected, flx.ToPrettyJson());
        }

        [Test]
        public void TestOffsetAndLengthAreOfTypeULong()
        {
            var json = @"{""channels_in"":64,""dilation_height_factor"":1,""dilation_width_factor"":1,""fused_activation_function"":1,""pad_values"":1,""padding"":0,""stride_height"":1,""stride_width"":1}";
            var flx = FlxValue.FromBytes(new byte[] {99, 104, 97, 110, 110, 101, 108, 115, 95, 105, 110, 0, 100, 105, 108, 97, 116, 105, 111, 110, 95, 104, 101, 105, 103, 104, 116, 95, 102, 97, 99, 116, 111, 114, 0, 100, 105, 108, 97, 116, 105, 111, 110, 95, 119, 105, 100, 116, 104, 95, 102, 97, 99, 116, 111, 114, 0, 102, 117, 115, 101, 100, 95, 97, 99, 116, 105, 118, 97, 116, 105, 111, 110, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 112, 97, 100, 95, 118, 97, 108, 117, 101, 115, 0, 112, 97, 100, 100, 105, 110, 103, 0, 115, 116, 114, 105, 100, 101, 95, 104, 101, 105, 103, 104, 116, 0, 115, 116, 114, 105, 100, 101, 95, 119, 105, 100, 116, 104, 0, 8, 130, 119, 97, 76, 51, 41, 34, 21, 8, 1, 8, 64, 1, 1, 1, 1, 0, 1, 1, 4, 4, 4, 4, 4, 4, 4, 4, 16, 36, 1});
            Assert.AreEqual(json, flx.ToJson);
        }

        private void CheckLong(long value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsLong);
        }
        
        private void CheckULong(ulong value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsULong);
        }
        
        private void CheckLongAsULong(long value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsULong);
        }
        
        private void CheckLongAsDouble(long value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsDouble);
        }
        
        private void CheckULongAsDouble(ulong value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsDouble);
        }
        
        private void CheckDouble(double value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsDouble);
        }
        
        private void CheckBool(bool value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsBool);
        }
        
        private void CheckLongAsBool(long value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value != 0, flx.AsBool);
        }
        
        private void CheckULongAsBool(ulong value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value != 0, flx.AsBool);
        }
        
        private void CheckString(string value)
        {
            var bytes = FlexBuffer.SingleValue(value);
            var flx = FlxValue.FromBytes(bytes);
            Assert.AreEqual(value, flx.AsString);
        }
    }
}