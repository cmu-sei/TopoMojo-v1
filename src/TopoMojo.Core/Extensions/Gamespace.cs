// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using TopoMojo.Extensions;

namespace TopoMojo.Models
{
    public static class ModelExtensions
    {
        public static void MergeVms(this GameState state, Vm[] vms)
        {
            foreach (Vm vm in vms)
            {
                string name = vm.Name.Untagged();
                VmState vs = state.Vms
                    .Where(t => t.Name == name && !t.Id.HasValue())
                    .FirstOrDefault();

                if (vs != null)
                {
                    vs.Id = vm.Id;
                    vs.IsRunning = vm.State == VmPowerState.Running;
                }
            }
        }

        public static void Grade(this Models.v2.QuestionSpec question, string submission)
        {
            if (string.IsNullOrWhiteSpace(submission))
            {
                question.IsCorrect |= false;
                question.IsGraded = true;
                return;
            }

            string[] a = question.Answer.ToLower().Replace(" ", "").Split('|');
            string b = submission.ToLower().Replace(" ", "");

            switch (question.Grader) {

                case (v2.AnswerGrader)AnswerGrader.MatchAny:
                question.IsCorrect = a.Contains(b);
                break;

                case (v2.AnswerGrader)AnswerGrader.MatchAll:
                question.IsCorrect = a.Intersect(
                    b.Split(new char[] { ',', ';', ':', '|'})
                ).ToArray().Length == a.Length;
                break;

                case (v2.AnswerGrader)AnswerGrader.Match:
                default:
                question.IsCorrect = a.First().Equals(b);
                break;
            }

            question.IsGraded = true;
        }

        public static void SetQuestionWeights(this Models.v2.VariantSpec spec)
        {
            var questions = spec.Sections.SelectMany(s => s.Questions).ToArray();
            var unweighted = questions.Where(q => q.Weight == 0).ToArray();
            float max = questions.Sum(q => q.Weight);
            if (unweighted.Any())
            {
                float val = (1 - max) / unweighted.Length;
                foreach(var q in unweighted.Take(unweighted.Length - 1))
                {
                    q.Weight = val;
                    max += val;
                }
                unweighted.Last().Weight = 1 - max;
            }
        }

        public static DateTime ResolveExpiration(this RegistrationRequest request, DateTime ts, int max)
        {
            if (max > 0)
                request.MaxMinutes = Math.Min(request.MaxMinutes, max);

            if (request.ExpirationTime == DateTime.MinValue)
                request.ExpirationTime = ts.AddMinutes(request.MaxMinutes);

            if (max > 0 && request.ExpirationTime.Subtract(ts).TotalMinutes > max)
                request.ExpirationTime = ts.AddMinutes(max);

            return request.ExpirationTime;
        }
    }
}
