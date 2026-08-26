/* The modern template merges this object into mermaid's own config, after it
   has picked `default` or `dark` to match the page. Only theme-neutral
   variables are set here — the accents read on either background, and the
   fills are left to whichever mermaid theme is in play. */
export default {
  mermaid: {
    themeVariables: {
      primaryBorderColor: "#1e90f8",
      lineColor: "#7a8894",
      fontFamily: "var(--bs-body-font-family)",
    },
  },
}
