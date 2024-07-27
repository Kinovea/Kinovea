
## Adding a new language

- In Weblate: Kinovea > Components > Root > Add translation. 
- This will add the language to all other components.
- Check that the other components are translatable and have strings. Sometimes they show up as 0 strings and in the language list have a little ghost saying "X strings are not being translated here". In that case: Manage > Repository maintenance > Danger Zone > Rescan all translation files in the Weblate repository.
- In Github: Merge Weblate PR into main project.
- In Kinovea: Under each project, Languages > Add existing item > pick the language .resx.
- In Kinovea: Add entry in Kinovea.Services > Language > LanguageManager.cs.
- The language name in its own language is based on what Wikipedia uses for this language.


## Translation cycle

- During development of the new version, strings are added in the code as literals. 
- The English locale is kept stable during the entire dev cycle.
- During this time translations are contributed and merged into master.
- Occasionally these translations are consolidated back to the branch of the released version.
- New point releases are made with the updated translations.
- At the end of the dev cycle, in Weblate, LOCK the translation.
- Collect all the new strings from the code and add them to English locale.


## Adding new strings

- Weblate: Lock
- VS: add new entries to the assembly resx reference file, ex: RootLang.resx
- VS: replace the string literals with the newly created resources.
- git: Commit + push.
- Weblate: Manage > Repository maintenance > Update.
- At this point the new strings should be visible and other languages should be marked as less than 100%.
- Weblate: translate the new strings to French, Spanish, Italian.
- Weblate: commit + push.
- git: merge Weblate PR.
- Pull and verify translations in context.
- Weblate: unlock.






