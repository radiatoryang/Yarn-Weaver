# Yarn-Weaver
<img width=30% align=right src=https://raw.githubusercontent.com/radiatoryang/Yarn-Weaver/master/yarnWeaver_sample.gif> 

A simple tool built in Unity C# to playtest / test-run [Yarn](https://github.com/InfiniteAmmoInc/Yarn) files! If you're not familiar, [Yarn](https://github.com/InfiniteAmmoInc/Yarn) is a Twine-like dialogue scripting engine + node editor tool that you can use for Unity games... I thought playtest functionality would've been integrated into the Yarn editor itself -- but unfortunately it's not there yet, so until then, you can use this tool to fill in that gap! 

## USAGE
1. keep the Yarn editor window open, and keep the Yarn Weaver window open at the same time
2. when you edit a script in Yarn editor, save it as a **yarn.txt** (.json is supported too, but it's harder to diff / read in a text editor)
3. ... and then in Yarn Weaver, open the file and/or refresh it to see new changes, and play!

## DOWNLOAD
- latest Windows and Mac OSX builds are here: https://github.com/radiatoryang/Yarn-Weaver/releases/
- there might be bugs with the "open file dialog" on certain versions of OSX

## IMPORTANT NOTE:
so, YarnSpinner doesn't really know which node in your Yarn file is the "start"... to try to figure it out, YarnWeaver searches your Yarn file for 2 things:
- a node title beginning with the word "Start" (or "start" or "START" or "sTaRt")
- a node title beginning with the filename (e.g. "Sally.json" would prompt a search for a node labeled "Sally")
- ... and if those searches fail, then it just starts with the first node it finds, which usually means the oldest node in your Yarn file

## HOW TO CONTRIBUTE
accepting pull requests here: https://github.com/radiatoryang/Yarn-Weaver/issues

ideally, post a comment or reply if you're working on something, so that we don't double-up on work

## uses the following:
- Yarn https://github.com/InfiniteAmmoInc/Yarn
- YarnSpinner https://github.com/thesecretlab/YarnSpinner/
- UnityStandaloneFileBrowser https://github.com/gkngkc/UnityStandaloneFileBrowser

## license?
MIT
