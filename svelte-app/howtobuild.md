https://stackoverflow.com/a/63536641

npm run build
npm run start
npx inliner http://localhost:5000/ | Out-String | Set-Content ..\src\veff\html\templates\inlined.html


