using System;
using System.Collections.Generic;
using IcpcResolver.UserControl;
using NUnit.Framework;

namespace IcpcResolver.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        private static void CopyProblemDtoTo(ProblemDto from, ProblemDto to)
        {
            to.Label = from.Label;
            to.Status = from.Status;
            to.Time = from.Time;
            to.Try = from.Try;
        }

        [Test]
        public void Test_PostInit()
        {
            var teamDto = new TeamDto
            {
                ProblemsFrom = new List<ProblemDto>
                {
                    new()
                    {
                        Try = 1,
                        Time = 123,
                        Status = ProblemStatus.Accept
                    }
                },
                ProblemsTo = new List<ProblemDto>
                {
                    new()
                    {
                        Try = 1,
                        Time = 123,
                        Status = ProblemStatus.Accept,
                    }
                }
            };
            var f = teamDto.ProblemsFrom[0];
            var t = teamDto.ProblemsTo[0];

            // Accept -> Accept => Accept -> Accept
            teamDto.PostInit();
            Assert.AreEqual(f, t);
            Assert.AreEqual(ProblemStatus.Accept, t.Status);

            // UnAccept -> UnAccept => UnAccept -> UnAccept
            f.Status = ProblemStatus.UnAccept;
            t.Status = ProblemStatus.UnAccept;
            teamDto.PostInit();
            Assert.AreEqual(f, t);
            Assert.AreEqual(ProblemStatus.UnAccept, t.Status);

            // Accept1 -> Accept2 => exception
            f.Status = ProblemStatus.Accept;
            CopyProblemDtoTo(f, t);
            t.Try += 1;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());

            f.Status = ProblemStatus.FirstBlood;
            CopyProblemDtoTo(f, t);
            t.Status = ProblemStatus.Accept;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());

            // Pending -> * => exception
            f.Status = ProblemStatus.Pending;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());

            // * -> Pending => exception
            f.Status = ProblemStatus.Accept;
            t.Status = ProblemStatus.Pending;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());

            // !IsAccept -> IsAccept => Pending -> IsAccept
            f.Status = ProblemStatus.NotTried;
            t.Status = ProblemStatus.Accept;
            t.Try = f.Try + 1;
            teamDto.PostInit();
            Assert.AreEqual(ProblemStatus.Pending, f.Status);
            Assert.AreEqual(ProblemStatus.Accept, t.Status);

            CopyProblemDtoTo(t, f);
            f.Status = ProblemStatus.UnAccept;
            t.Status = ProblemStatus.FirstBlood;
            t.Try = f.Try + 1;
            teamDto.PostInit();
            Assert.AreEqual(ProblemStatus.Pending, f.Status);
            Assert.AreEqual(ProblemStatus.FirstBlood, t.Status);

            // !IsAccept -> !IsAccept => !IsAccept -> !IsAccept
            f.Status = ProblemStatus.UnAccept;
            CopyProblemDtoTo(f, t);
            teamDto.PostInit();
            Assert.AreEqual(f, t);
            Assert.AreEqual(ProblemStatus.UnAccept, t.Status);

            // !IsAccept1 -> !IsAccept2 => Pending -> !IsAccept2
            t.Try = f.Try + 1;
            teamDto.PostInit();
            Assert.AreEqual(ProblemStatus.Pending, f.Status);
            Assert.AreEqual(ProblemStatus.UnAccept, t.Status);
            
            // status A -> status B => exception
            f.Status = ProblemStatus.UnAccept;
            CopyProblemDtoTo(f, t);
            t.Status = ProblemStatus.Accept;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());
            
            // Try checker
            f.Status = ProblemStatus.UnAccept;
            f.Try = 2;
            CopyProblemDtoTo(f, t);
            t.Status = ProblemStatus.Accept;
            t.Try = 1;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());

            // Time checker
            f.Status = ProblemStatus.UnAccept;
            f.Time = 100;
            CopyProblemDtoTo(f, t);
            t.Status = ProblemStatus.Accept;
            t.Time = 1;
            Assert.Throws<ArgumentException>(() => teamDto.PostInit());
        }
    }
}