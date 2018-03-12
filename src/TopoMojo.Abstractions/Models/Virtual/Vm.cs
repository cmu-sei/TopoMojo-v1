using System;

namespace TopoMojo.Models.Virtual
{
    public class Vm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Reference { get; set; }
        public string DiskPath { get; set; }
        public string Stats { get; set; }
        public string Status { get; set; }
        public string GroupName { get; set; }
        public VmPowerState State { get; set; }
        public VmQuestion Question { get; set; }
        public VmTask Task { get; set; }
    }

    public enum VmPowerState { off, running, suspended}

    public class VmQuestion
    {
        public string Id { get; set; }
        public string Prompt { get; set; }
        public string DefaultChoice { get; set; }
        public VmQuestionChoice[] Choices { get; set; }
    }

    public class VmAnswer
    {
        public string QuestionId { get; set; }
        public string ChoiceKey { get; set; }
    }

    public class VmQuestionChoice
    {
        public string Key { get; set; }
        public string Label { get; set; }
    }
    public class VmTask
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Progress { get; set; }
        public DateTime WhenCreated { get; set; }
    }

    public class DisplayInfo
    {
        public string Id { get; set; }
        public string TopoId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Conditions { get; set; }
    }

}
