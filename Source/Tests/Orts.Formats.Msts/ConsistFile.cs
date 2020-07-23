﻿// COPYRIGHT 2020 by the Open Rails project.
// 
// This file is part of Open Rails.
// 
// Open Rails is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Open Rails is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Open Rails.  If not, see <http://www.gnu.org/licenses/>.

using Orts.Formats.Msts;
using ORTS.Common;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Tests.Orts.Formats.Msts
{
    public class ConsistFileTests
    {
        private static readonly IDictionary<string, string> Folders = new Dictionary<string, string>();

        [Fact]
        public static void TestTrainProperties()
        {
            ITrainFile train;
            using (TestContent content = new TestContent())
                train = new ConsistFile(MakeTestFile(content));

            Assert.Equal("Test consist", train.DisplayName);
            Assert.Equal(36.65728f, train.MaxVelocityMpS);
            Assert.Equal(0.5f, train.Durability);
            Assert.True(train.PlayerDrivable);
        }

        [Fact]
        public static void TestForwardWagonReferences()
        {
            using (TestContent content = new TestContent())
            {
                ITrainFile train = new ConsistFile(MakeTestFile(content));
                var expected = new WagonReference[]
                {
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2bnsfcar", "US2BNSFCAR.wag"), false, 0),
                    new WagonReference(Path.Combine(content.TrainsetPath, "dash9", "DASH9.eng"), false, 1),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), false, 2),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), false, 3),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), false, 4),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), false, 5),
                    new WagonReference(Path.Combine(content.TrainsetPath, "gp38", "GP38.eng"), true, 6),
                };
                Assert.Equal(expected, train.GetForwardWagonList(content.Path, Folders));
            }
        }

        [Fact]
        public static void TestReverseWagonReferences()
        {
            using (TestContent content = new TestContent())
            {
                ITrainFile train = new ConsistFile(MakeTestFile(content));
                var expected = new WagonReference[]
                {
                    // For the moment, we use the UiD's as entered into the .con file; no reversing.
                    new WagonReference(Path.Combine(content.TrainsetPath, "gp38", "GP38.eng"), false, 6),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), true, 5),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), true, 4),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), true, 3),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2graincar", "US2GRAINCAR.wag"), true, 2),
                    new WagonReference(Path.Combine(content.TrainsetPath, "dash9", "DASH9.eng"), true, 1),
                    new WagonReference(Path.Combine(content.TrainsetPath, "us2bnsfcar", "US2BNSFCAR.wag"), true, 0),
                };
                Assert.Equal(expected, train.GetReverseWagonList(content.Path, Folders));
            }
        }

        [Fact]
        public static void TestForwardLocomotiveChoices()
        {
            using (TestContent content = new TestContent())
            {
                ITrainFile train = new ConsistFile(MakeTestFile(content));
                var locomotive = new PreferredLocomotive(Path.Combine(content.TrainsetPath, "dash9", "DASH9.eng"));
                Assert.Equal(new HashSet<PreferredLocomotive>() { locomotive }, train.GetLeadLocomotiveChoices(content.Path, Folders));
            }
        }

        [Fact]
        public static void TestReverseLocomotiveChoices()
        {
            using (TestContent content = new TestContent())
            {
                ITrainFile train = new ConsistFile(MakeTestFile(content));
                var locomotive = new PreferredLocomotive(Path.Combine(content.TrainsetPath, "gp38", "GP38.eng"));
                Assert.Equal(new HashSet<PreferredLocomotive>() { locomotive }, train.GetReverseLocomotiveChoices(content.Path, Folders));
            }
        }

        [Fact]
        public static void TestNoForwardWagonReferencesGivenUnsatisifablePreference()
        {
            using (TestContent content = new TestContent())
            {
                ITrainFile train = new ConsistFile(MakeTestFile(content));
                var unsatisfiable = new PreferredLocomotive(Path.Combine(content.TrainsetPath, "acela", "acela.eng"));
                Assert.Empty(train.GetForwardWagonList(content.Path, Folders, preference: unsatisfiable));
            }
        }

        [Fact]
        public static void TestNoReverseWagonReferencesGivenUnsatisifablePreference()
        {
            using (TestContent content = new TestContent())
            {
                ITrainFile train = new ConsistFile(MakeTestFile(content));
                var unsatisfiable = new PreferredLocomotive(Path.Combine(content.TrainsetPath, "acela", "acela.eng"));
                Assert.Empty(train.GetReverseWagonList(content.Path, Folders, preference: unsatisfiable));
            }
        }

        private static string MakeTestFile(TestContent content)
        {
            const string text = @"SIMISA@@@@@@@@@@JINX0D0t______

Train (
	TrainCfg ( ""test""
		Name(""Test consist"")
		Serial(1)
		MaxVelocity(36.65728 1.00000)
		NextWagonUID(7)
		Durability(0.50000)
		Wagon(
			WagonData(us2bnsfcar US2BNSFCAR)
			UiD(0)
		)
		Engine(
			UiD(1)
			EngineData(dash9 DASH9)
		)
		Wagon(
			WagonData(us2graincar US2GRAINCAR)
			UiD(2)
		)
		Wagon(
			WagonData(us2graincar US2GRAINCAR)
			UiD(3)
		)
		Wagon(
			WagonData(us2graincar US2GRAINCAR)
			UiD(4)
		)
		Wagon(
			WagonData(us2graincar US2GRAINCAR)
			UiD(5)
		)
		Engine(
			Flip()
			UiD(6)
			EngineData(gp38 GP38)
		)
	)
)";
            string path = Path.Combine(content.ConsistsPath, "test.con");
            File.WriteAllText(path, text);
            return path;
        }
    }
}
