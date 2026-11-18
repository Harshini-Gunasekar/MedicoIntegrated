/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}"
  ],
  theme: {
    extend: {
       colors: {
            "primary": "#4F46E5",
            "on-primary": "#ffffff",
            "primary-container": "#4F46E5",
            "on-primary-container": "#ffffff",
            "background": "#F8FAFC",
            "surface": "#F8FAFC",
            "surface-container": "#F1F5F9",
            "surface-container-low": "#F1F5F9",
            "surface-container-lowest": "#ffffff",
            "surface-container-high": "#E2E8F0",
            "on-surface": "#0F172A",
            "on-surface-variant": "#475569",
            "outline": "#94A3B8",
            "outline-variant": "#E2E8F0",
            "secondary": "#1E293B",
            "on-secondary": "#ffffff",
            "secondary-container": "#334155",
            "on-secondary-container": "#ffffff",
            "tertiary": "#64748B",
            "error": "#EF4444",
            "navy": {
                800: "#1E293B",
                900: "#0F172A",
            }
        },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/container-queries')
  ],
}
