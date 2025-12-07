
## Adding a new language

- In Weblate: Kinovea > Components > Root > Add translation. 
- This will add the language to all other components.
- Check that the other components are translatable and have strings. 
    Sometimes they show up as 0 strings and in the language list have a little ghost saying "X strings are not being translated here".
    This seems to happen when people add strings to the glossary. This automatically adds them to the English glossary.
    Normally there shouldn't be any string in the glossary, we don't use this feature. They can be removed from the English glossary.
    Otherwise: Manage > Repository maintenance > Danger Zone > Rescan all translation files in the Weblate repository.
- In Github: Merge Weblate PR into main project.
- In Kinovea: Under each project, Languages > Add existing item > pick the language .resx.
- In Kinovea: Add entry in Kinovea.Services > Language > LanguageManager.cs.
- The language name in its own language is based on what Wikipedia uses for this language.


## Translation cycle

- Code: during development of the new version, strings are added as literals. 
- Weblate: the English locale is kept stable during the entire dev cycle.
- Weblate/git: during this time translations are contributed and merged into master.
- git: Occasionally these translations are consolidated back to the branch of the released version.
- git: New point releases are made with the updated translations.
- Weblate: At the end of the dev cycle, in Weblate, LOCK the translation.
- Code: Collect all the new strings from the code and add them to English locale.


## Adding new strings

- Weblate: Lock
- VS: use ResXManager: https://marketplace.visualstudio.com/items?itemName=TomEnglert.ResXManager
- VS: View > Other window > ResXManager
- VS: Identify all new strings litterals and do right click > Move to resource.
- git: Commit + push.
- Weblate: Manage > Repository maintenance > Update.
- Weblate: At this point the new strings should be visible and other languages should be marked as less than 100%.
- Weblate: Translate the new strings to French, Spanish, Italian.
- Weblate: commit + push.
- git: merge Weblate PR.
- Pull and verify translations in context.
- Weblate: unlock.






