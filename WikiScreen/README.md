

Chrome 61 needed 

scale factor mast be same in `MakeWikiScreen` and command below

```
Start-Process -FilePath 'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe' -ArgumentList '--remote-debugging-port=9222','--user-data-dir=C:\myChromeUser', '--high-dpi-support=1', '--force-device-scale-factor=1'
```