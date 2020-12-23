﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Conflux.Web3.Accounts
{
/****
    Copyright(c) 2014 Metaco SA

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.


    *****/


    /// <summary>
    /// Represent a path in the hierarchy of HD keys (BIP32)
    /// </summary>
    public class KeyPath
    {
        public KeyPath()
        {
            _Indexes = new uint[0];
        }

        /// <summary>
        /// Parse a KeyPath
        /// </summary>
        /// <param name="path">The KeyPath formated like 10/0/2'/3</param>
        /// <returns></returns>
        public static KeyPath Parse(string path)
        {
            var parts = path
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p != "m")
                .Select(ParseCore)
                .ToArray();
            return new KeyPath(parts);
        }

        public KeyPath(string path)
        {
            _Indexes =
                path
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p != "m")
                .Select(ParseCore)
                .ToArray();
        }

        private static uint ParseCore(string i)
        {
            bool hardened = i.EndsWith("'");
            var nonhardened = hardened ? i.Substring(0, i.Length - 1) : i;
            var index = uint.Parse(nonhardened);
            return hardened ? index | 0x80000000u : index;
        }

        public KeyPath(params uint[] indexes)
        {
            _Indexes = indexes;
        }

        readonly uint[] _Indexes;
        public uint this[int index]
        {
            get
            {
                return _Indexes[index];
            }
        }

        public uint[] Indexes
        {
            get
            {
                return _Indexes.ToArray();
            }
        }

        public KeyPath Derive(int index, bool hardened)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "the index can't be negative");
            uint realIndex = (uint)index;
            realIndex = hardened ? realIndex | 0x80000000u : realIndex;
            return Derive(new KeyPath(realIndex));
        }

        public KeyPath Derive(string path)
        {
            return Derive(new KeyPath(path));
        }

        public KeyPath Derive(uint index)
        {
            return Derive(new KeyPath(index));
        }

        public KeyPath Derive(KeyPath derivation)
        {
            return new KeyPath(
                _Indexes
                .Concat(derivation._Indexes)
                .ToArray());
        }

        public KeyPath Parent
        {
            get
            {
                if (_Indexes.Length == 0)
                    return null;
                return new KeyPath(_Indexes.Take(_Indexes.Length - 1).ToArray());
            }
        }

        public KeyPath Increment()
        {
            if (_Indexes.Length == 0)
                return null;
            var indices = _Indexes.ToArray();
            indices[indices.Length - 1]++;
            return new KeyPath(indices);
        }

        public override bool Equals(object obj)
        {
            KeyPath item = obj as KeyPath;
            if (item == null)
                return false;
            return ToString().Equals(item.ToString());
        }
        public static bool operator ==(KeyPath a, KeyPath b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.ToString() == b.ToString();
        }

        public static bool operator !=(KeyPath a, KeyPath b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        string _Path;
        public override string ToString()
        {
            return _Path ?? (_Path = string.Join("/", _Indexes.Select(ToString).ToArray()));
        }

        private static string ToString(uint i)
        {
            var hardened = (i & 0x80000000u) != 0;
            var nonhardened = (i & ~0x80000000u);
            return hardened ? nonhardened + "'" : nonhardened.ToString(CultureInfo.InvariantCulture);
        }

        public bool IsHardened
        {
            get
            {
                if (_Indexes.Length == 0)
                    throw new InvalidOperationException("No indice found in this KeyPath");
                return (_Indexes[_Indexes.Length - 1] & 0x80000000u) != 0;
            }
        }
    }
}


