using System;
using System.Linq;

namespace TopoMojo.vSphere
{
    public static class FilterFactory
    {

        public static PropertyFilterSpec[] VmFilter(ManagedObjectReference mor, string props = "summary layout resourcePool")
        {
            props += " resourcePool";
            PropertySpec prop = new PropertySpec {
                type = "VirtualMachine",
                pathSet = props.Split(new char[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray()
            };

            ObjectSpec objectspec = new ObjectSpec {
                obj = mor, //_vms or vm-mor
                selectSet = new SelectionSpec[] {
                    new TraversalSpec {
                        type = "Folder",
                        path = "childEntity"
                    }
                }
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public static PropertyFilterSpec[] TaskFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec {
                type = "Task",
                pathSet = new string[] {"info"}
            };

            ObjectSpec objectspec = new ObjectSpec {
                obj = mor, //task-mor
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public static PropertyFilterSpec[] VmNetFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec {
                type = "Network",
                pathSet = new string[] {"name", "vm"}
            };

            ObjectSpec objectspec = new ObjectSpec {
                obj = mor, //task-mor
                selectSet = new SelectionSpec[] {
                    new TraversalSpec {
                        type = "ComputeResource",
                        path = "network"
                    }
                }
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public static PropertyFilterSpec[] NetworkFilter(ManagedObjectReference mor)
        {
            return NetworkFilter(mor, "networkInfo.portgroup networkInfo.vswitch");
        }
        public static PropertyFilterSpec[] NetworkFilter(ManagedObjectReference mor, string props)
        {
            PropertySpec prop = new PropertySpec {
                type = "HostNetworkSystem",
                pathSet = props.Split(new char[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
            };

            ObjectSpec objectspec = new ObjectSpec {
                obj = mor, //_net
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public static PropertyFilterSpec[] DatastoreFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec {
                type = "Datastore",
                pathSet = new string[] { "browser", "summary" }
            };

            ObjectSpec objectspec = new ObjectSpec {
                obj = mor, //_res
                selectSet = new SelectionSpec[] {
                    new TraversalSpec {
                        type = "ComputeResource",
                        path = "datastore"
                    }
                }
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public static PropertyFilterSpec[] ResourceFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec {
                type = "ResourcePool",
                pathSet = new string[] {"runtime"}
            };

            ObjectSpec objectspec = new ObjectSpec {
                obj = mor, //_pool
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

    }
}
