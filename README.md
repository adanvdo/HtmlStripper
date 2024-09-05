# HtmlStripper
An ugly and poorly written console application for stripping elements from local Html files

## Configuration

This application uses a JSON file to configure the types of elements you want to remove from each HTML file.
You can see an example configuration below:

### ***remove***  
  
 0 = Entire Element  
 1 = Child Elements  
 2 = Attributes and Child Elements  

```
{
    "class":[
        { "name": "nav", "remove":0 },
        { "name": "footer", "remove":1 }
    ],
    "tag":[
        { "name": "script", "remove":0 },
        { "name": "meta", "remove":0 },
        { "name": "title", "remove":1 },
        { "name": "ins", "remove":0 }
    ],
    "id":[
        {"name": "search", "remove":0}
    ],
    "other":[
        {"name":"comment", "remove":0}
    ]
}
```

With this example configuration, HtmlStripper will search each file for:
 - elements with the `class` attribute value "nav" or "footer"
 - "script", "meta", "title" or "ins" elements
 - elements with the id `search`
 - html comments
 
 ### class
 a list of class names. html nodes will be matched if they have a matching class attribute
 
 ### tag
 a list of element/node names (i.e. div, img, etc). html nodes will be matched if they are the same element

 ### id
 a list of element ids. html nodes will be matched if they have a matching id attribute
 
 ### other
 a list of node types. html nodes will be matched if they have a matching element type

## How To Use

### Prompts
If you launch HtmlStripper without any command-line arguments, it will prompt you to specify:
 1. The path to the directory containing the html files you want to process.
 2. The path to the json configuration file
  
### Command-Line arguments
If you launch HtmlStripper from the command-line, you can specify the directory and json paths to avoid being prompted  
`htmlstripper.exe -dir=C:\Website\Pages -json=C:\Website\striplist.json -copydir=C:\Website\Copies`
