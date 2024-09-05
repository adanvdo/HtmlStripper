# HtmlStripper
An ugly and poorly written console application for stripping elements from local Html files

## Configuration

This application uses a JSON file to configure the types of elements you want to remove from each HTML file.
You can see an example configuration here:  

```
{
	"class":[
		"nav",
		"footer"
	],
	"tag":[
		"script",
		"meta",
		"title",
		"ins"
	],
	"other":[
		"comment"
	]
}
```

With this configuration, HtmlStripper will search each file for:
 - elements with the `class` attribute value "nav" or "footer"
 - "script", "meta", "title" or "ins" elements
 - html comments
 
 ### class
 a list of class names. html nodes will be matched if they have a matching class attribute
 
 ### tag
 a list of element/node names (i.e. div, img, etc). html nodes will be matched if they are the same element
 
 ### other
 a list of node types. html nodes will be matched if they have a matching element type

## How to uses

### Prompts
If you launch HtmlStripper without any command-line arguments, it will prompt you to specify:
 1. The path to the directory containing the html files you want to process.
 2. The path to the json configuration file
 
### Command-Line arguments
If you launch HtmlStripper from the command-line, you can specify the directory and json paths to avoid being prompted
`htmlstripper.exe -dir=C:\Website\Pages -json=C:\Website\striplist.json`