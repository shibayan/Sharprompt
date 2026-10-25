import { readFile, writeFile, readdir, mkdir, access } from 'node:fs/promises'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'

const docs = fileURLToPath(new URL('../', import.meta.url))
const output = resolve(docs, '.vitepress/dist')
const base = 'https://shibayan.github.io/Sharprompt/'

async function listPages(directory, prefix = '') {
  const pages = []
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    const path = `${prefix}${entry.name}`
    if (entry.isDirectory()) pages.push(...await listPages(resolve(directory, entry.name), `${path}/`))
    else if (entry.name.endsWith('.md') && entry.name !== 'index.md') pages.push(path)
  }
  return pages.sort()
}

const pages = []
for (const locale of ['en', 'ja']) {
  for (const file of await listPages(resolve(docs, locale))) {
    pages.push({ source: `${locale}/${file}`, path: locale === 'en' ? file : `ja/${file}` })
  }
}

const routes = new Map(pages.map(page => [`/${page.path.replace(/\.md$/, '')}`, `${base}${page.path}`]))
for (const page of pages) {
  let markdown = (await readFile(resolve(docs, page.source), 'utf8'))
    .replace(/^\uFEFF/, '')
    .replace(/\r\n/g, '\n')
    .replace(/^---\r?\n[\s\S]*?\r?\n---\r?\n/, '')

  // Expand the same complete C# files imported by VitePress so Markdown clients see real code.
  const imports = [...markdown.matchAll(/^<<< @\/([^\r\n]+)\r?$/gm)]
  for (const match of imports) {
    const source = await readFile(resolve(docs, match[1]), 'utf8')
    markdown = markdown.replace(match[0], `\`\`\`csharp\n${source.replace(/^\uFEFF/, '').replace(/\r\n/g, '\n').trimEnd()}\n\`\`\``)
  }
  markdown = markdown
    .replace(/^:::.*\r?$/gm, '')
    .replace(/^(```\w+) \[[^\]]+\]\r?$/gm, '$1')
    .replace(/\]\((\/[^)#\s]*)(#[^)]*)?\)/g, (match, path, anchor = '') =>
      routes.has(path) ? `](${routes.get(path)}${anchor})` : match)

  page.title = markdown.match(/^# (.+)\r?$/m)?.[1] ?? page.path
  const destination = resolve(output, page.path)
  // Fail if routing and export paths drift apart.
  await access(destination.replace(/\.md$/, '.html'))
  await mkdir(dirname(destination), { recursive: true })
  await writeFile(destination, markdown.trim() + '\n')
}

const index = [
  '# Sharprompt',
  '',
  '> Synchronous interactive command-line prompts for C# and .NET 8 / .NET 10.',
  '',
  'Install the Sharprompt NuGet package and import Sharprompt. Source generators and XML API documentation are bundled.',
  'The default driver requires terminal input and output. Use a scripted console driver for unattended verification.',
  'Start with the usage guide for API choices, defaults, model constraints, and cancellation. All links below provide Markdown.',
  '',
]
for (const [locale, heading] of [['en', 'Guides and Reference'], ['ja', 'Japanese Guides and Reference']]) {
  index.push(`## ${heading}`, '')
  const localized = pages.filter(page => page.source.startsWith(`${locale}/`))
  for (const first of ['usage-guide.md', 'getting-started.md', 'testing.md']) {
    const page = localized.find(page => page.source === `${locale}/${first}`)
    index.push(`- [${page.title}](${base}${page.path})`)
  }
  for (const page of localized.filter(page => !['usage-guide.md', 'getting-started.md', 'testing.md'].includes(page.source.slice(3)))) {
    index.push(`- [${page.title}](${base}${page.path})`)
  }
  index.push('')
}
await writeFile(resolve(output, 'llms.txt'), index.join('\n'))
console.log(`Exported ${pages.length} Markdown pages and llms.txt.`)
