# Documentation Development

The VitePress site uses `en/` for English sources (rewritten to the site root) and `ja/` for Japanese sources. Keep both languages aligned. The main-site navigation is in `.vitepress/config.mts`.

```sh
cd docs
npm ci
npm run docs:build
```

The build exports HTML first, then `scripts/export-markdown.mjs` creates a Markdown counterpart for each guide and reference page and generates `llms.txt` from the same sources. The home pages use VitePress frontmatter and are excluded. Output is under `.vitepress/dist/`, with English Markdown at `usage-guide.md`, Japanese Markdown at `ja/usage-guide.md`, and the index at `llms.txt`. Pages advertise Markdown through `rel="alternate"` links and the index through `rel="describedby"`.

The exporter expands complete-file C# imports, removes VitePress containers and code-group labels, and rewrites internal page links to the published Markdown URLs. Add new guides to the navigation; they will be indexed automatically. Do not hand-edit generated exports. The testing guides import the compiled `samples/Sharprompt.Automation` sources so their code stays synchronized.

The documentation workflows also run when the imported sample changes. The .NET build workflow compiles README examples and runs the automation sample for both target frameworks.
