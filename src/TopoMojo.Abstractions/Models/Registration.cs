// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Collections.Generic;

namespace TopoMojo.Models
{
    public class Registration
    {
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string ResourceId { get; set; }
        public string GamespaceId { get; set; }
        public string ClientId { get; set; }
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public Challenge Challenge { get; set; }
    }

    public class RegistrationRequest
    {
        public string ResourceId { get; set; }
        public string ClientId { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int Variant { get; set; }
        public int MaxAttempts { get; set; }
        public int MaxMinutes { get; set; }
        public int Points { get; set; } = 100;
        public bool AllowReset { get; set; }
        public bool AllowPreview { get; set; }
        public bool StartGamespace { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    public class ChallengeSpec
    {
        public ICollection<QuestionSpec> Questions { get; set; } = new List<QuestionSpec>();
        public Dictionary<string,string> Randoms { get; set; } = new Dictionary<string, string>();
    }

    public class QuestionSpec
    {
        public string Text { get; set; }
        public string Hint { get; set; }
        public string Answer { get; set; }
        public float Weight { get; set; }
        public AnswerGraderOld Grader { get; set; }
    }

    public class Challenge
    {
        public string GamespaceId { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public float Score { get; set; }
    }

    public class Question
    {
        public string Text { get; set; }
        public string Hint { get; set; }
        public string Answer { get; set; }
        public float Weight { get; set; }
        public bool IsCorrect { get; set; }
    }

    public enum AnswerGraderOld
    {
        Match,
        MatchAll,
        MatchAny
    }
}
