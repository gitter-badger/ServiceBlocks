# ServiceBlocks
Toolbox for building scalable .NET services

Features included:
Engines:
 * Queued Task Pool - scheduler that implements producer/consumer pattern and allows to synchronize tasks by key.
 Runs a consumer task pool of limited capacity and supports concurrent producer threads.
 
 * Command Processor - based on Queued task Pool, engine for concurrent execution of commands over specific state.
 Can be used to implement asynchronous command pattern and as base for CQRS and Event Sourcing designs
 
 Failover Cluster:
 * Implementation of failover cluster with two instances - primary and backup and autimatic failover.
 
 Messaging:
 * Wrappers that assist to make pub/sub communication with topics, snapshots, filters and corrections
 
 Examples:
 

Open source projects used:
* NetMq
* Automatonymous State Machine


