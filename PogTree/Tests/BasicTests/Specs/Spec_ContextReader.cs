﻿/**Copyright (c) 2023 Richard H Stannard

This source code is licensed under the MIT license found in the
LICENSE file in the root directory of this source tree.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using PogTreeTest.Theories;

namespace PogTreeTest.Specs
{
    public class Spec_ContextReader
    {
        protected ITestOutputHelper _output = null;

        public Spec_ContextReader(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(Theory_ContextReader))]
        public void BasicReaderTest(TestIterationArgs args)
        {
            PogTreeTestHelper.CompareReaderResults(args);
        }

        [Theory]
        [ClassData(typeof(Theory_ContextReaderReverse))]
        public void BasicReaderTestReverse(TestIterationArgs args)
        {
            PogTreeTestHelper.CompareReaderResultsReverse(args);
        }
    }
}
