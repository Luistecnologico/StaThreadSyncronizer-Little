# StaThreadSyncronizer-Little
 A little version of StaThreadSyncronize,
 a DLL to be able to call from any thread through a unique-same thread in .NET. Little version only provide functionalities Get, but its reduced code makes it lighter and easier to adapt to the needs of each.

## How do we swtich between two threads?
- Thread 1: Sends a messsage to acommon list.
- Thread 2: Listens for incoming messages from the common list. In this case, Thread 2 will be an STA thread.

## Blocking filum class
I need a list to send work items from thread X to my STA thread. I also have to remove/read items only when there are items. So, I will pause the Peek method to wait until something pops into the list. On Internet, especially in GitHub, this is called a Blocking. 

- Because this list is used by two more threads, notice that I am blocking access to the list using the lock statement. 
- The Peek method is blocking until there is an item in the List. The way this works is by having a semaphore that counts all the items in the list. When the BlokingFilum is created for the first time, the semaphore and list is empty, so calling Peek will block. 

`private Semaphore mSemaphore = new Semaphore(0, int.MaxValue);`

- When I try to Peek an item, I block / Pause the method with a `WaitHandles (WaitHandle.WaitAny(mWaitHandles);)`. This code means "Wait until there is an item, or until the read thread is marked to stop running." 
- The STA thread must be the reading thread, spending time waiting for an item on the list or processing a message from the list. 
- Notice that when a item is added into the list, it releases the semaphore, indicating that a resource is available; this will cause the Peek method to unblock. 

##SendOrPostCallbackItem class
The BlockingFilum class is generic, so you can re-use it in another application. Considering this list is responsible to send code from one thread to another, the ideal item to list is a delegate. In this case the ideal delegate is `SendOrPostCallback` delegate.

- `SendOrPostCallbackItem` contains the delegate we wish to execute on the STA thread.
- The `Send` is a really helper methods, it is both responsible for launching the code, and it is designed to be called from the STA thread. `Send` is required to block, and to report exceptions back, so I use a `ManualResentEvent` when the execution is complete. If there are any exception, I also keep track of the it and it will be thrown on the producer thread. 

Overall, this class storing the delegate to execute and executing it. 

(writting...)
