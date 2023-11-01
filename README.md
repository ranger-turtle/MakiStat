# MakiStat

<img align="right" width="160px" height="160px" src="MakiSei/MakiStat icon.ico">

MakiStat is an application and a template-based framework designed for generating small yet
expanding multilingual websites. It requires relatively minimal technical knowledge
and does not require any initial configuration to run it.

> **Warning** <br>
> This documentation covers the beta version of the MakiStat and its features may be changed in the future.

## General

MakiStat is a static site generator designed for developing small-scale websites in multiple
languages and using data occuring in multiple places. MakiStat uses HTML templates
powered by Scriban template engine which provides full-fledged programming language ([see this repository
for more info](https://github.com/scriban/scriban)).
Thanks to it, templates can be written like HTML with embedded back-end code but the
output pages have fixed content and cannot be randomized unless the JavaScript is included.
MakiStat fills pages with data stored in JSON files.

MakiStat is also a framework providing separation the information exclusive to particular
pages and information used in multiple pages. In general, it supports the structure consisting
of the ```skeleton.html``` file which is the main template used for all the pages on the website and
```_global``` and ```_main``` folders which store templates and the data. MakiStat generates pages in the
predefined output folder which has to be in the same folder as skeleton and the website in 
the default language is placed directly in the output folder. Language versions are placed in subfolders
and their structure is generally the same as the default language website.
MakiStat cannot directly handle non-HTML resources (such as CSS, scripts and images)
and you need to store them in the output folder and maintain them separately,
using already generated website.

MakiStat can be used for websites which are expected to be slightly expanded in the future
and use relatively little data of the same kind they would be included in database, where
maintenance of the database and the CMS would be too cumbersome and expensive.
Data can be stored in JSON files and used by multiple pages, although it is up to
the author how they should be organized. It can be also used for making mock-ups of
larger-scale websites before designing and deploying the database and other complex
web technologies.

MakiStat application itself is very easy to use since it is an ordinary desktop application
and once the structure of the website and the data have been already created, the website can
be generated in two simple steps.

Writing the structure itself require only the basics of programming and JSON syntax and
you do not need to know any other frameworks. MakiStat does not give any ready layout which you
have to modify for your needs what would be difficult when your layout needs to be very different
from the provided.

Generated website can be manually published to the traditional hosting services.

## Framework

The data and the templates need to be organized in the two folders: ```_global``` and ```_main```.
Only files which can be outside are the skeleton.html which store the main layout of all of the pages
and .json files of the same name which contain elementary data (such as strings and numbers)
meant to be put to the skeleton template.

### Minimal required structure of the website framework
```
.
├── _global                 # Resources used across entire website
├── _main                   # Resources related to output pages
├── output                  # Folder for output website
├── _skeleton.html
└── _skeleton.default.json
```

If you want to learn on how to write your first website for MakiStat, go to [tutorial section](#Tutorial).

### ```_global``` folder

Folder intended to store templates and data used in multiple pages and not related to the specific page.

### ```_main``` folder

Folder indented to store website source files which directly represent specific pages. Page
templates can read partials and data from both ```_global``` and ```_main``` folders, although it is a good
practice to put the data not being a part of the specific page to be put in ```_global``` folder.
The folder structure in this folder is mostly mirrored in the output folder. MakiStat seeks page templates
only in this folder.

### ```output``` folder

Folder intended to be destination point of the website. The output has almost exactly the same folder
structure as ```_main``` folder. Only difference is the presence of folders named after language codes which
store the same website in different languages.

## Source file types

There are several types of file making up the "blueprint" of the final static website. 

### Templates

Templates contain HTML and Scriban code. Scriban provides full-featured programming language
which can be used for embedding PHP or Razor-like scripts providing non-standard ways to handle
content meant to be put in output pages. MakiStat template engine can, besides putting data itself,
read data from other JSON files and render the content as it would be preprocessed with
back-end code by a web server. They must always have the ```.html``` extension.

#### Skeleton

This is the the skeleton layout template of every page in the website. It contains HTML elements used in every
page and has references to the sections written in page templates. MakiStat inserts the processed page
sections in place of these references. Simple language-variable data are read from
language-variable json files of the same name. There can be exactly one skeleton in whole structure,
needs to have the name ```_skeleton.html``` and must be placed in the same folder as
three framework folders.

This is also the entry point for MakiStat from where it starts the generation of the website.

#### Page template

Template representing the actual page expected to be in the output. They can be put only in ```_main``` folder,
since MakiStat seeks for these templates only in that folder. Its name must not begin with ```_``` character,
since MakiStat recognizes such files as partials and does not process them like the pages.

If it is placed in the subfolder chain, MakiStat generates the same subfolder chain and puts the page as
is placed in ```_main``` folder. The name of the page is also the same as its page template.
For example, MakiStat generates page ```_main/blogs/kitchen.html``` to the destination
```output/blogs/kitchen.html```.

Pages are divided into multiple user-defined sections put to the different places in the skeleton.

This is only template type the data from language version independent JSON files are inserted to, since
the single page can have the same data, regardless of the language version but not the same as in others,
such as image paths.

#### Partial

Partial is a template for the part of the page and cannot be used as standalone page template itself.
Its name must begin from underscore character to be skipped by MakiStat during website generation and not
treated as the output page. The good practice is to place partials in the ```_global``` folder,
since they are not intended to be the part of only one page.

### Data files

Data are saved in JSON files and loaded to the pages by the MakiStat. They represent simple and sometimes
translateable data which differ across pages and their language versions.
They must always have ```.json``` extension and have the same name as the template if they are
associated with and placed in the same folder e.g. language-independent data loaded to the
page ```kitchen.html``` need to have name ```kitchen.json```.

#### Language version independent data files

Data which always are the same in all of the language versions of the same page but still specific to
that one. They have simple ```.json``` extension. Data meant to be stored in such files are file paths,
numbers, URLs, data used for scripts etc. They can be used only with page templates.

#### Language version variable data files

Data containing data specific for the given language version. Unlike language-independent data, they
can be put to the all of the types of template. Each template needs to have at least one language version
variable data file and needs to have ```[lang code].json``` compound extension, where ```lang code```
is the language code. It can have predefined ```default``` value if the language version is meant to be
put to the root of the website or the user-defined code associated with specific language version. For example,
the data for ```index.html``` page in default language need to be defined in ```kitchen.default.json```
and has to be in the same folder. However, if there is ```index.pl.json``` file in the same folder, the
same page in different language version will be generated to ```output/pl/index.html```.

Presence of language JSON files for skeleton template determines what language versions will be generated.
For example, if there are some ```pl.[lang_code].json``` in the ```_main``` folder, but the
```_skeleton.pl.json``` file is absent, language version with ```pl``` code will not be generated.

If the version language JSON file for skeleton is available but some web page does not have equivalent
template, the page in that specific language version will not be generated e.g. when the
```_skeleton.pl.json``` file exists, but the ```exclusive.pl.json``` does not, the ```exclusive.html```
page in version associated with ```pl``` language code will not be generated and the right warning will
be printed in ```website.log``` file placed in the root of the website blueprint.

#### Independent data files

Data files not associated with specific template. They are meant to store information used by multiple
templates and can be loaded using function ```load_model```. They can be named in any way, although it is
recommended to be placed in the ```_global``` folder.

## MakiStat-only variables and functions

Most features of the MakiStat API are provided by the Scriban template engine which is a third-party
API and its documentation is available at https://github.com/scriban/scriban#documentation . That is why this
section covers API features exclusive to MakiStat. They are defined using tools provided by Scriban.

### Variables

#### ```global```

Object containing data read from skeleton data file associated with processed language version.

#### ```uni_page```

Object containing data read from the language-independent data file of the same name as the page template.
This object is scoped to the page and the skeleton.

#### ```page```

Object containing data read from the language variable data file of the same template. 
This object is scoped to the page and the skeleton.

#### ```current_page```

Name of the currently processed page, without extension.

#### ```lang_code```

Current lang code. Its type is string.

#### ```lang_dir_path```

Directory of the language version of the website. If the language code has value of ```default```,
its the root of the website.

#### ```main_path```

Absolute path to the ```_main``` folder.

#### ```global_path```

Absolute path to the ```_global``` folder.

#### ```uni_model```

Object containing language-independed data passed to the ```load_partial``` function.
This object is scoped to the partial.

#### ```model```

Object containing language-variable data passed to the ```load_partial``` function.
This object is scoped to the partial.

#### ```partial```

Object containing language-variable data exclusive to the partial and read from the file of the same
name and placement.
This object is scoped to the partial.

### Functions

#### ```load_page (template_name, section_name)```

It processes the page template and renders its section of the name passed through parameter
```section_name```. It must be called only on skeleton template.

- ```template_name```: string containing path to the page template. ```.html``` extension is added automatically.
- ```section_name```: string containing section declared in the page template.

__Returns__: processed section of the page.

#### ```load_partial (template_name, model, uni_model)```

It processes the partial and renders it. It accepts two objects with language-independent and
language-variable data.

##### Parameters

- ```template_name```: string containing path to the partial template. .html extension is added automatically.
- ```model```: object containing language-variable data to be passed to partial
- ```uni_model```: object containing language independent data to be passed to partial

__Returns__: processed partial.

#### ```load_model (model_path, multilingual)```

Loads data from a JSON file. Depending on a value of ```multilingual``` parameter, it reads
language-variable or language independent data.
Language is determined by language code of currently processed language.

##### Parameters

- ```model_path```: string containing path to the JSON file. The name itself should not even contain
language code from the language-variable data JSON extension because of the second argument.
- ```multilingual```: boolean determining if the data must be read from language-variable or language
independent data.

__Returns__: object containing data read from JSON file.

#### ```load_lang_codes ()```

__Returns__: array containing available language codes. They are useful when you generate list of the
language versions of the current page.

#### ```load_lang_page_url (lang_code)```

This function returns path to the processed page in language version associated with given language code.
It can be useful for generating paths in the list of the same page in available languages.

##### Parameters

- ```lang_code```: language code needed to determine the path of the processed page.

__Returns__: path to the page in language version associated with given language code.

## Website generation algorithm

First, it reads the ```skeleton.html``` file which is the starting point for MakiStat. For each skeleton language data file,
it downloads global data specific for the language of code saved in the file extension.
For each page which is not generated, to which one of the needed resources have been modified
before or is not registered in ```check.msmc```, the page is generated.
MakiStat recognizes ```html``` files as page templates as long as their names do not begin
with underscore. First, the data from the page are inserted into the skeleton.
After that, data from partials and JSON files referred in the page templates and
the partials are processed and put to the page in the memory. If the page processing is successful,
its contents are saved to the file. If some error occurs, entire generation process is stopped.

## Tutorial

This tutorial is written for the MakiStat beginners but already familiar with HTML and Scriban language.

### Creating main folders

First, choose your folder where you want to create wesbite blueprint for MakiStat.
Nextly, create three folders: ```_global```, ```_main``` and ```output```.

### Skeleton

Create ```_skeleton.html``` in the same folder. Fill it with this code:

```html {.line-numbers}
<!DOCTYPE html>
<html lang="{{ if lang_code == 'default'
'en'
else
lang_code
end }}">
<head>
	<meta charset="UTF-8">
	<meta http-equiv="X-UA-Compatible" content="IE=edge">
	<meta name="viewport" content="width=device-width, initial-scale=1.0">
	<title>{{ page.title }} - Great Cooking</title>
	<link rel="preconnect" href="https://fonts.googleapis.com">
	<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
	<link href="https://fonts.googleapis.com/css2?family=DynaPuff:wght@400;600&display=swap" rel="stylesheet">
	<link rel="stylesheet" href="/style.css">
	{{- load_page page_file 'stylesheets' ~}}
</head>
<body>
	<div id="main">
			<header>Great Cooking</header>
			<main>
			{{- load_page page_file 'main' ~}}
			</main>
			<footer>
				<p style="margin-bottom: 0.4em;">{{ global.copyright }}</p>
				<div class="lang-menu">
					<ul>
						<li><a href="{{ load_lang_page_url 'pl' }}" >PL</a></li>
						<li><a href="{{ load_lang_page_url 'default' }}" >EN</a></li>
					</ul>
				</div>
			</footer>
		</div>
</body>
</html>
```

Code fragments between double curly braces and the braces themselves will be replaced by the content
in the output page. The content can be read from data saved in JSON files or entire web page fragment
processed from child templates. Skeleton nests sections saved in page templates and the page templates
themselves can nests partials.

After MakiStat read the skeleton, it reads and processes each page template which needs to be
in ```_main``` folder and its name cannot begin with underscore, since templates with such names
are recognized as partials. For more information, how the pages are processed,
see [this chapter](#website-generation-algorithm).

Code written as value of ```lang``` attribute is replaced by MakiStat with the value of
```lang_code``` which stores current language code which was read from the first member of
compound extension of one of ```_skeleton.[lang code].json``` files which store language-dependent
data scoped to all templates in the project. If its value is ```default```, MakiStat puts ```en```
value here, since in this tutorial, English is default language of the website and the version in such
language will be put in the root folder. Other language versions are put in subfolders whose name will
be exactly the same as their language codes.

```page.title``` is replaced by the data saved in ```title``` property in the ```page``` global
object which store data exclusive for both the page and the language version.

page data file with the
currently processed language. Language is determined by language code read from the compound extension
 of the JSON file named ```_skeleton```.

Source line of code ```load_page page_file 'main'``` is actually a function call - ```load_page```
is a predefined function name and ```page_file``` and ```'main'``` are parameters.
There are no parentheses and commas, since Scriban template engine uses language based on Ruby which
does not use such symbols in function calls.

```load_page``` actually puts the section saved in currently processed web page. ```page_file``` is
predefined variable which stores the path of the processed page. Second parameter of the ```load_page```
is the name of the section defined in the page template.

```global.copyright``` is replaced by the data saved in ```copyright``` property in the
```global``` global object which store information different across the language versions but
always the same in all pages of the specific language version.

```load_lang_page_url 'pl'``` call renders the URL leading to the same page in Polish language
whose code is 'pl'. Similar call in line below renders the same URL in English language.

### Language version variable global data files

Number of such files determine number of language versions of the same website and one of them always
must have ```default``` lang code.

In the tutorial, we will create bilingual website - in English and Polish language. Therefore, we need
to create two data files which will be referred by the skeleton template and therefore, will be on every
page. However, they are different across language versions. Give them names ```_skeleton.default.json```
and ```_skeleton.pl.json```. Fill the first one with this code:

```json
{
	"copyright": "All Rights Reserved"
}
```

and the second one with:

```json
{
	"copyright": "Wszelkie Prawa Zastrzeżone"
}
```

```default``` code means that the language version of the website will be stored in website root.
The default language defined in this tutorial is English. Polish language version will be stored
in ```pl``` subfolder, since language code of Polish version is ```pl```.

Code in both files is very short, since we use only one multilingual entry which will be used in all pages.
Language-variable data files should have generally the same set of properties, although additional
or missing properties are acceptable as long as the template code will handle these non-standard cases.

Data from one of the ```_skeleton.[lang code].json``` files is written to the ```global``` global
variable by MakiStat. The order of languages the MakiStat processes depends of the order of file names read
from file system.

### Template of the index page

Create template of the index page in ```_main``` folder and name it ```index.html```.
Fill it with this code:

```html
{{~ if section == 'main' ~}}
	<h1>{{ page.header }}</h1>
	<p>{{ page.description }}</p>
	<p>{{ page.suggestion }} <a href="recipes.html">{{ page.link }}</a></p>
{{~ end ~}}
```

First line of source code declares ```main``` section and the content before line containing ```end```
will be rendered in place of ```{{- load_page page_file 'main' ~}}``` line code in ```_skeleton.html```.
Sections are not mandatory and can be skipped, like ```stylesheet``` section in this example. When
section is not declared, MakiStat will leave empty place.

### Default language data file specific to index page

Create ```index.default.json``` file in the same folder as ```index.html``` page template file.
Fill it with code:

```json
{
	"title": "Home Page",
	"header": "Welcome to Great Cooking!",
	"description": "Here you find some quick'n'easy dish recipes!",
	"suggestion": "For most popular recipes, ",
	"link": "go here"
}
```

There is no ```page.title``` reference in ```index.html``` template, so why this JSON
has ```title``` property has been included? Value assigned to ```title``` will be put to the
reference which you have added in ```_skeleton.html```.

### Web page templates using partial

Many pages may contain components having the same HTML tag structure and they can be saved in
partial templates. Create ```recipes``` folder in ```_main``` folder and create three page templates:
```fizzy.html```, ```sandwich.html``` and ```orange_rice.html```. Fill each of them with HTML code:

```html
{{~ if section == 'stylesheets'}}
	<link rel="stylesheet" href="/recipes/recipes.css">
{{~ end -}}
{{~ if section == 'main'
load_partial (global_path + '/_recipe') page uni_page
end ~}}
```

Why has every file exactly the same content? It is because the page uses ```_recipe``` HTML partial
template which needs to be placed in ```_global``` folder whose path is stored in ```global_path```
global variable. Partial is processed and loaded by ```load_partial``` function which passes three
parameters: path to the partial template, ```page``` global object and ```uni_page``` global object
which unlike ```page``` object, it stores information used in all language versions but still exclusive to
currently processed page. Data stored in these objects will be put to the partial.

These pages use additional stylesheet and that's why ```stylesheets``` section is included here.

### Partial templates

Create ```_recipe.html``` partial template in the ```_global``` folder.
Although MakiStat does not seek this folder for the pages, it is still good to put trailing underscore
in the partial file name to avoid confusion with page templates. Fill this file with this content:

```html
<h1>{{ model.header }}</h1>
<img src="/img/{{ uni_model.image }}.jpg" alt="{{ partial.illustration }}" loading="lazy">
<section>
	<h2>{{ partial.ingredientsHeader }}</h2>
	<ul class="ingredients">
		{{ for $ingredient in model.ingredients ~}}
		<li>{{ $ingredient }}</li>
		{{~ end }}
	</ul>
</section>
<section>
	<h2>{{ partial.stepsHeader }}</h2>
	<ol class="recipe">
		{{ for $step in model.steps ~}}
		<li>{{ $step }}</li>
		{{~ end }}
	</ol>
</section>
```

Structure of template is similar to the page template, but it uses ```model``` and ```uni_model```
instead of ```page``` and ```uni_page```, respectively. These variables are two last parameters
of ```load_partial``` function which was called in parent template. ```load_partial``` passes
data from files which are mentioned below.

### Data passed to partial

As with ```index.html```, you need to create ```fizzy.default.json```, ```sandwich.default.json```
and ```orange_rice.html``` for each page template in ```recipes``` folder. Fill them:

```fizzy.default.json```:
```json
{
	"title": "Fizzy drink",
	"header": "Fizzy drink from powder",
	"ingredients":
	[
		"Water",
		"Fizzy drink powder"
	],
	"steps":
	[
		"Fill the glass with water.",
		"Warm the water in microwave oven for 30 seconds.",
		"Put the powdered drink to the water and mix well."
	]
}
```

```sandwich.default.json```:
```json
{
	"title": "Sandwich with cheese and tomatoes",
	"header": "Fizzy drink from powder",
	"ingredients":
	[
		"Bread",
		"Cheese",
		"Tomatoes"
	],
	"steps":
	[
		"Slice bread, cheese and tomatoes.",
		"Put the slices of cheese and tomato on bread and put the slice of bread on them.",
		"Repeat two previous steps for remaining sandwiches."
	]
}
```

```orange_rice.default.json```:
```json
{
	"title": "Puffed Rice",
	"header": "Puffed Rice with Orange Syrup",
	"ingredients":
	[
		"Puffed rice",
		"Orange syrup"
	],
	"steps":
	[
		"Put the puffed rice to the bowl.",
		"Pour the syrup on the rice.",
		"Mix the rice with the syrup well."
	]
}
```

However, they will not be
enough - data for these pages are passed to the partial coming from the same file and there is another
problem - these pages include image path which would be the same in all language versions. Writing the same
entry in each language variable data file would cause redundancy which would be particularly troublesome
when the website needs to be in many languages. And the path cannot be directly written in partial which
is loaded from single file but used by multiple page templates. That is why you need to create
```fizzy.json```, ```sandwich.json``` and ```orange_rice.json``` files which store information
which are language-independent but they still exclusive to the page, although they are meant to put to
the templates other than page templates MakiStat loads these data to them first. These data are loaded
to ```uni_page``` global object and they need to be passed to ```uni_model``` parameter. Fill them: 

```fizzy.json```:
```json
{
	"image": "fizzy"
}
```

```sandwich.json```:
```json
{
	"image": "sandwich"
}
```

```orange_rice.json```:
```json
{
	"image": "rice"
}
```


### Partial multilingual data files

As you might have noticed, partials themselves may contain information which would be in multiple languages.
You need to create language-variable data files which are analogous to the data files read to page templates
i.e. they share the same main part of name and they are in the same folder as the partial. These data are
loaded to object ```partial``` which can be used only in partials.
Create file ```_recipe.default.json``` in the same folder as ```_partial``` and fill with code:

```json
{
	"illustration": "ilustracja",
	"ingredientsHeader": "Ingredients",
	"stepsHeader": "Steps"
}
```

### Recipe list

Create file ```recipes.html``` in ```_main``` folder and fill it with this code:

```html
{{~ if section == 'main'}}
<h1>{{ page.title }}</h1>
<p>{{ page.description }}:</p>
<ul class="recipe-list">
	{{ for $link in uni_page.links
	$recipeData = load_model main_path + '/recipes/' + $link }}
	<li><a href="recipes/{{ $link }}.html">{{ $recipeData.title }}</a></li>
	{{ end }}
</ul>
{{~ end }}
```

And create such data files:
```recipe.default.json```
```json
{
	"title": "Recipes",
	"description": "There are all recipes available on the website"
}
```

```recipe.json```
```json
{
	"links":
	[
		"fizzy",
		"sandwich",
		"orange_rice"
	]
}
```

Why there are recipe page template file names instead of titles in ```recipes.json```?
Titles are already saved in the language data variable
files associated with recipe page templates and writing the same set of informations in multiple places
is too redundant. That is why ```recipes.html``` template calls ```load_model``` function in order
 to read informations from ```recipes/fizzy.[current lang code].json```,
```recipes/sandwich.[current lang code].json``` and
```recipes/orange_rice.[current lang code].json``` files and saves them to ```$recipeData```
in each iteration. This is another situation, where language-independent data files come in handy - links
have the same value, no matter of the processed language and it might be changed in future.
These values can be directly embedded into template script but it would be not really convenient.

Thanks to ```load_model```, you can use data JSON files as the database substitute, but without
automated consistency and validation.

### Polish version of the website

All data files with Polish language content are stored in ```*.pl.json``` files.
They differ from ```*.default.json``` files only with language of the values and they are stored in the
same folder. You can copy them from demo which you can download from repository.

### CSS stylesheets and scripts

MakiStat is designed to handle HTML code only and therefore, you need to put stylesheets and scripts
manually to the output. Fortunately, you can copy stylesheets from demo.

### Generating the website

Now the website is ready for generation! Only things you need to do is to launch MakiStat, click Search
button, choose ```_skeleton.html``` file and click Generate! button. Your website is generated!

> **Note** <br>
> The good news is that MakiStat is intelligent and does not generate pages when resources used
> for generation such pages are not changed.
