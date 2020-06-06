# DistributedStateTest
Test app for experimenting with a distributed object app on [LiteNetLib](https://github.com/RevenantX/LiteNetLib).

This is a .NET Standard application with unit testing, intended as an experimental testbed for
a distributed LAN system I'm working on that wants both reliable and unreliable peer-to-peer
networking, locally only.

At the moment (as of early June 2020) the library is under very active development and is by no
means feature complete, even for my own use.  It does at least have a GitHub workflow to make sure
the tests pass!

## Architecture

The basic idea is each app in the peer-to-peer system will instantiate a DistributedPeer (class
I implemented in this library), which represents that app's endpoint in the peer-to-peer system.
DistributedPeers announce their existence periodically via UDP broadcast to a known port, including
a list of other peers they know about already.

They also listen on that port, and when any peer hears from a new peer that doesn't know them yet,
it responds. The new peer then set up a LiteNetLib peer-to-peer connection to the respondent. So
it's an all-way N-to-N peer network. (Which won't scale much, but doesn't have to, as my app is
currently local-wifi only with a max of maybe 5 or 6 nodes.)

Each node can create owner objects, which it hosts. Owner objects, when created, send create-proxy
messages to all peers. So the peers wind up with proxies for all owned objects on other nodes.
(Newly connected peers also get proxies for all existing objects owned by other peers, naturally.)

Both owner objects and proxies wrap "local" objects which actually instantiate the interesting
behavior. Messages (e.g. gameplay events, user commands, etc.) sent to owner objects get relayed
reliably to proxies; messages sent to proxies get relayed to the owner, which decides what to do
(the command may be stale so the owner may ignore it, etc.).

So the owner object is authoritative over the state of its proxies, and the proxies are all kept in
lockstep with its state.

## Project

The project is divided into three libraries:

- DistributedStateLib (the generic distributed peer and object code)
- DistributedThing (an example of instantiating a particular kind of distributed object)
- DistributedStateTest (testing assembly that covers both the generic code and the specific Thing code)

## Motivation

I am working on a networked version of my [Holofunk](http://holofunk.com) mixed reality music
system. I need a flexible, low-latency way to handle relatively low-bandwidth drop-in-drop-out
networking.  I know there are Unity shared object protocols, but I need something that I can tune
a little more tightly than that.

This will be integrated with my [NowSoundLib](https://github.com/RobJellinghaus/NowSoundLib) project
as well, in the Holofunk context.
