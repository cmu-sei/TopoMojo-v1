// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo.Abstractions
{
    public class VmOperation
    {
        public string Id { get; set; }
        public VmOperationType Type { get; set; }
        public int WorkspaceId { get; set; }
    }

    public enum VmOperationType
    {
        Start,
        Stop,
        Save,
        Revert,
        Delete
    }
}
