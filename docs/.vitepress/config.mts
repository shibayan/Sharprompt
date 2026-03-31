import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'Sharprompt',
  description: 'Interactive command-line based application framework for C#',
  base: '/Sharprompt/',
  cleanUrls: true,
  head: [
    ['link', { rel: 'icon', href: '/Sharprompt/icon.png' }]
  ],
  rewrites: {
    'en/:rest*': ':rest*'
  },
  locales: {
    root: {
      label: 'English',
      lang: 'en',
      themeConfig: {
        nav: [
          { text: 'Guide', link: '/getting-started' },
          { text: 'NuGet', link: 'https://www.nuget.org/packages/Sharprompt/' }
        ],
        sidebar: [
          {
            text: 'Introduction',
            items: [
              { text: 'Getting Started', link: '/getting-started' }
            ]
          },
          {
            text: 'Prompt Types',
            items: [
              { text: 'Input', link: '/prompt-types/input' },
              { text: 'Confirm', link: '/prompt-types/confirm' },
              { text: 'Password', link: '/prompt-types/password' },
              { text: 'Select', link: '/prompt-types/select' },
              { text: 'MultiSelect', link: '/prompt-types/multi-select' },
              { text: 'List', link: '/prompt-types/list' },
              { text: 'Bind', link: '/prompt-types/bind' }
            ]
          },
          {
            text: 'Reference',
            items: [
              { text: 'Validators', link: '/validators' },
              { text: 'Configuration', link: '/configuration' },
              { text: 'Advanced Features', link: '/advanced' }
            ]
          }
        ]
      }
    },
    ja: {
      label: '日本語',
      lang: 'ja',
      themeConfig: {
        nav: [
          { text: 'ガイド', link: '/ja/getting-started' },
          { text: 'NuGet', link: 'https://www.nuget.org/packages/Sharprompt/' }
        ],
        sidebar: [
          {
            text: 'Introduction',
            items: [
              { text: 'Getting Started', link: '/ja/getting-started' }
            ]
          },
          {
            text: 'Prompt Types',
            items: [
              { text: 'Input', link: '/ja/prompt-types/input' },
              { text: 'Confirm', link: '/ja/prompt-types/confirm' },
              { text: 'Password', link: '/ja/prompt-types/password' },
              { text: 'Select', link: '/ja/prompt-types/select' },
              { text: 'MultiSelect', link: '/ja/prompt-types/multi-select' },
              { text: 'List', link: '/ja/prompt-types/list' },
              { text: 'Bind', link: '/ja/prompt-types/bind' }
            ]
          },
          {
            text: 'Reference',
            items: [
              { text: 'Validators', link: '/ja/validators' },
              { text: 'Configuration', link: '/ja/configuration' },
              { text: 'Advanced Features', link: '/ja/advanced' }
            ]
          }
        ]
      }
    }
  },
  themeConfig: {
    logo: '/icon.png',
    socialLinks: [
      { icon: 'github', link: 'https://github.com/shibayan/Sharprompt' }
    ],
    search: {
      provider: 'local'
    },
    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © Tatsuro Shibamura'
    }
  }
})
