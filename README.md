# Yarn-Weaver
<img width=30% align=left src=https://raw.githubusercontent.com/radiatoryang/Yarn-Weaver/master/yarnWeaver_sample.gif> 
simple tool built in Unity C# to playtest / test-run [Yarn](https://github.com/InfiniteAmmoInc/Yarn) files (if you're not familiar, [Yarn](https://github.com/InfiniteAmmoInc/Yarn) is a Twine-like dialogue scripting engine + node editor tool that you can use for Unity games)... I thought this functionality would've been in the Yarn editor itself -- but it's not, so until then, there's this tool to fill in that gap! My suggested workflow is this: 

1. keep the Yarn editor window open, and keep the Yarn Weaver window open at the same time
2. when you edit a script in Yarn editor, save it as JSON...
3. ... and then in Yarn Weaver, open the file and/or refresh it to see new changes, and play!

## RELEASE BUILDS:
- latest Windows and Mac OSX builds are here: https://github.com/radiatoryang/Yarn-Weaver/releases/
- there might be bugs with the "open file dialog" on certain versions of OSX

## IMPORTANT NOTE:
so, YarnSpinner doesn't really know which node in your Yarn file is the "start"... to try to figure it out, YarnWeaver searches your Yarn file for 2 things:
- a node that starts with the word "Start"
- a node that starts with the filename (e.g. "Sally.json" would prompt a search for a node labeled "Sally")
- ... and if those searches fail, then it just starts with the first node it finds, which usually means the oldest node in your Yarn file

### uses the following:
- YarnSpinner https://github.com/thesecretlab/YarnSpinner/
- UnityStandaloneFileBrowser https://github.com/gkngkc/UnityStandaloneFileBrowser

### license?
MIT
