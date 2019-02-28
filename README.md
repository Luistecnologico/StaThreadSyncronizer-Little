# StaThreadSyncronizer-Little
 A little version of StaThreadSyncronizer
 A DLL to be able to call from any thread through a unique-same thread in .NET. Little version only provide functionalities Get, but its reduced code makes it lighter and easier to adapt to the needs of each.

# How do we swtich between two threads?
- Thread 1: Sends a messsage to acommon list.
- Thread 2: Listens for incoming messages from the common list. In this case, Thread 2 will be an STA thread.

(writing...)
