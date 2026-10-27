---
name: FinanceAgent
description: A night-sky glass console for personal finance, with three lit tiles and one cash-flow horizon.
colors:
  bg-0: "#110d36"
  bg-1: "#1c1752"
  surface: "rgba(255, 255, 255, .055)"
  surface-2: "rgba(255, 255, 255, .09)"
  surface-solid: "#1d1950"
  border: "rgba(206, 200, 255, .14)"
  border-strong: "rgba(206, 200, 255, .26)"
  text: "#f2f0ff"
  text-2: "#c3bfeb"
  text-3: "#9a95cf"
  accent: "#8b7bff"
  accent-2: "#a99cff"
  accent-strong: "#6d5cf2"
  link: "#b9afff"
  mint: "#4fd6a5"
  coral: "#ff7f8c"
  amber: "#ffc46b"
  series-income: "#4fd6a5"
  series-spend: "#ff9a6b"
  tile-income-a: "#e2faf0"
  tile-income-b: "#b8eed8"
  tile-income-ink: "#0e3527"
  tile-income-line: "#22a877"
  tile-spend-a: "#fff0e7"
  tile-spend-b: "#ffcfb6"
  tile-spend-ink: "#4a1d0c"
  tile-spend-line: "#f0763a"
  tile-net-a: "#e9ebff"
  tile-net-b: "#c6ccff"
  tile-net-ink: "#1b2160"
  tile-net-line: "#5465f0"
typography:
  display:
    fontFamily: "Manrope, ui-sans-serif, system-ui, -apple-system, Segoe UI, Roboto, sans-serif"
    fontSize: "clamp(2rem, 3.4vw, 2.9rem)"
    fontWeight: 800
    lineHeight: 1.08
    letterSpacing: "-0.03em"
  figure:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "clamp(1.7rem, 2.4vw, 2.25rem)"
    fontWeight: 800
    lineHeight: 1.1
    letterSpacing: "-0.03em"
    fontFeature: "tnum"
  headline:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "1.75rem"
    fontWeight: 800
    letterSpacing: "-0.02em"
  title:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "1.02rem"
    fontWeight: 750
    letterSpacing: "-0.01em"
  body:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "15px"
    fontWeight: 400
    lineHeight: 1.55
  body-strong:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "0.92rem"
    fontWeight: 700
  meta:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "0.78rem"
    fontWeight: 400
    lineHeight: 1.35
  label:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 700
    letterSpacing: "0.06em"
rounded:
  xs: "6px"
  sm: "10px"
  md: "12px"
  DEFAULT: "14px"
  lg: "22px"
  pill: "999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "14px"
  gutter: "20px"
  panel-x: "22px"
  page-x: "32px"
components:
  button-primary:
    backgroundColor: "{colors.accent-strong}"
    textColor: "#ffffff"
    rounded: "{rounded.md}"
    padding: "0.5rem 1rem"
  button-primary-hover:
    backgroundColor: "#7c6cff"
    textColor: "#ffffff"
  button-outline:
    backgroundColor: "rgba(255, 255, 255, .04)"
    textColor: "{colors.text}"
    rounded: "{rounded.md}"
    padding: "0.5rem 1rem"
  button-outline-hover:
    backgroundColor: "rgba(255, 255, 255, .1)"
    textColor: "#ffffff"
  button-danger:
    backgroundColor: "{colors.coral}"
    textColor: "#2a0710"
    rounded: "{rounded.md}"
    padding: "0.5rem 1rem"
  button-sm:
    rounded: "{rounded.sm}"
    padding: "0.3rem 0.7rem"
  input:
    backgroundColor: "rgba(255, 255, 255, .06)"
    textColor: "{colors.text}"
    rounded: "{rounded.md}"
    padding: "0.55rem 0.85rem"
  input-focus:
    backgroundColor: "rgba(255, 255, 255, .09)"
  search-field:
    backgroundColor: "rgba(255, 255, 255, .06)"
    textColor: "{colors.text-3}"
    rounded: "{rounded.DEFAULT}"
    height: "46px"
    padding: "0 16px"
  nav-item:
    textColor: "{colors.text-2}"
    rounded: "{rounded.md}"
    padding: "11px 12px"
  nav-item-active:
    textColor: "#ffffff"
  panel:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.lg}"
    padding: "20px 22px"
  tile-income:
    backgroundColor: "{colors.tile-income-a}"
    textColor: "{colors.tile-income-ink}"
    rounded: "{rounded.lg}"
    padding: "20px 22px 70px"
  tile-spend:
    backgroundColor: "{colors.tile-spend-a}"
    textColor: "{colors.tile-spend-ink}"
    rounded: "{rounded.lg}"
    padding: "20px 22px 70px"
  tile-net:
    backgroundColor: "{colors.tile-net-a}"
    textColor: "{colors.tile-net-ink}"
    rounded: "{rounded.lg}"
    padding: "20px 22px 70px"
  attention-row:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.DEFAULT}"
    padding: "12px 14px"
  attention-row-hover:
    backgroundColor: "{colors.surface-2}"
    textColor: "#ffffff"
  txn-row:
    backgroundColor: "rgba(255, 255, 255, .04)"
    rounded: "{rounded.md}"
    padding: "10px 12px"
  tag:
    backgroundColor: "rgba(255, 255, 255, .07)"
    textColor: "{colors.text-2}"
    typography: "{typography.label}"
    rounded: "{rounded.pill}"
    padding: "0.12rem 0.55rem"
  chart-readout:
    backgroundColor: "rgba(30, 26, 78, .62)"
    textColor: "{colors.text-2}"
    rounded: "{rounded.DEFAULT}"
    padding: "12px 14px"
    width: "196px"
  auth-card:
    backgroundColor: "rgba(30, 26, 78, .55)"
    textColor: "{colors.text}"
    rounded: "{rounded.lg}"
    padding: "32px"
---

# Design System: FinanceAgent

## Overview

**Creative North Star: "The Night-Sky Glass Console"**

FinanceAgent is a dark, lit instrument. A deep indigo ground carries soft violet, blue, and pink glows fixed to the viewport; every working surface floats on it as frosted glass with a single pale 1px border. Against that night, the month's three headline numbers (income, spending, net) are the only light-ground objects on the screen: pastel tiles that each carry their own area history along the bottom edge. Everything else stays in the glass register so those three tiles, and the cash-flow horizon beneath them, read first.

The density is that of an operating console, not a marketing page: a 268px sidebar, a top bar with search and an alerts chip, then a 12-column grid of panels with 20px gutters. One family (Manrope) does all the talking, heavy and tightly tracked for figures and headings, regular for prose, with tabular numerals wherever money appears. Icons are a single drawn set on a 24px grid at a 1.75 stroke, so every glyph shares one hand.

The world is a deliberate refusal of the grey admin template: colour carries meaning (violet for action and selection, mint for money in and good change, coral for money out, flags, and danger, amber for warnings) rather than decoration.

**Key Characteristics:**
- Indigo night ground with fixed radial glows; glass panels with blur and a 1px pale-lavender border.
- Three pastel KPI tiles (mint, peach, periwinkle) are the only light surfaces in the signed-in app.
- Violet primary; mint up, coral down, amber warn; series colours repeat across tiles, chart, and legend.
- Manrope throughout, 800 weight for figures and page titles, tabular numerals for all money.
- Generous rounding (12px controls, 14px rows, 22px containers); line icons at 1.75 stroke.
- Charts are server-rendered SVG with HTML readouts, not a chart library.

## Colors

A cool, saturated night palette: indigo grounds, violet light, and three semantic hues that always mean the same thing.

### Primary
- **Ultraviolet Signal** (accent-strong): the primary button fill, checked checkboxes, the left stop of budget bars. The one colour that says "act here."
- **Lavender Glow** (accent): selection highlight, the active nav item's gradient, chart default stroke, the first donut segment, and the violet tone of focus shadows.
- **Pale Lavender** (accent-2): focus-visible outline, caret, empty-state icons, the light end of budget bars.
- **Moonlit Link** (link): inline links and link buttons; hover lifts to near-white.

### Secondary
- **Ledger Mint** (mint, series-income): income line and swatch, positive money text (lightened to #6fe3b8 for text on dark), success alerts, the "good" attention tone, valid-field outline.

### Tertiary
- **Flag Coral** (coral): danger buttons, flagged-charge rows, the alerts count badge, invalid-field outline, over-budget bars; negative money text is lightened to #ff9ca6.
- **Spend Apricot** (series-spend): the spending series on the cash-flow chart and legend; warmer than coral so "spent" never reads as "error."
- **Caution Amber** (amber): warning alerts, near-limit budget bars, suggestion icons, the review-queue attention tone.

### Neutral
- **Midnight Indigo** (bg-0) and **Dusk Indigo** (bg-1): the page ground, laid as a 165deg gradient under four fixed radial glows.
- **Glass** (surface) and **Glass Raised** (surface-2): panel and row fills at rest and on hover.
- **Solid Indigo** (surface-solid): where glass cannot work (select option lists, the badge's cut-out ring).
- **Haze Border** (border) and **Haze Border Strong** (border-strong): the 1px edge on every glass surface; strong for table heads, outline buttons, tooltips, and the auth card.
- **Starlight** (text), **Mist** (text-2), **Dim Mist** (text-3): primary text, secondary text and labels, and metadata, axis ticks, placeholders. Headings are pure white.

### KPI tile pastels
Each tile is a 140deg gradient from its `-a` to `-b` stop, set in its own dark `-ink`, with its area spark drawn in its `-line` colour: income in mint (tile-income-*), spending in peach (tile-spend-*), net in periwinkle (tile-net-*). Deltas inside tiles use #0b6b47 for good and #a3243a for bad, chosen for contrast on the pastel.

### Named Rules
**The Meaning-Not-Decoration Rule.** Mint, coral, and amber are reserved for money direction and status. Never use them as ornament; a coral element must be something the user should look at.

**The Three Lights Rule.** Light-ground surfaces in the signed-in shell are the three KPI tiles only. New panels stay in glass.

**The One Series Rule.** Income is always mint and spending is always apricot, in tiles, chart lines, hover dots, and legend swatches alike.

## Typography

**Display Font:** Manrope (with ui-sans-serif, system-ui, Segoe UI, Roboto fallbacks)
**Body Font:** Manrope
**Label/Mono Font:** none distinct; numerals use Manrope's tabular figures.

**Character:** A single geometric-humanist sans used at two temperatures: heavy (750 to 800) and tightly tracked for every number and heading, plain 400 to 600 for prose and labels. Weight, not a second family, creates hierarchy.

### Hierarchy
- **Display** (800, clamp 2rem to 2.9rem, 1.08): the sign-in intro headline only, balanced, max 14ch.
- **Figure** (800, clamp 1.7rem to 2.25rem, 1.1, -0.03em, tabular): KPI tile values; the donut centre value uses the same voice at 1.45rem.
- **Headline** (800, 1.75rem; 1.5rem under 760px): page titles in the page head; auth card title at 1.6rem.
- **Title** (750, 1.02rem, -0.01em): panel headings.
- **Body** (400, 15px, 1.55): prose; page subtitles in Mist, max 68ch.
- **Body strong** (700, 0.9 to 0.92rem): row titles, merchant names, tile labels.
- **Meta** (400 to 600, 0.72 to 0.8rem): row metadata, chart axes, legends, captions, in Dim Mist.
- **Label** (700, 0.75rem, 0.06em, uppercase): table column headers only.

### Named Rules
**The Tabular Money Rule.** Every currency value is set with tabular numerals and never wraps.

**The Header-Only Caps Rule.** Uppercase tracked text exists for table column headers. It is not used for section labels above headings.

## Layout

The signed-in shell is a two-column grid: a sticky 268px sidebar (inset 18px, the nav panel filling the viewport height) and a fluid main column with a top bar (18px vertical, 32px right, 28px left) and content padded 4px 32px 48px 28px. Every page opens with a page head: title and Mist subtitle on the left, actions (selects, primary button) bottom-aligned on the right, 22px above the content.

The dashboard is a 12-column grid with 20px gaps: three 4-column tiles, then an 8/4 split (cash-flow chart beside the attention list), then mixed spans for budgets, donut, and recent transactions. Panels stack with 20px between them on list pages; filter bars are panels with tighter 14px 18px padding.

Responsive: at 1100px wide spans collapse to full width while tiles keep three across; at 960px the sidebar becomes an off-canvas drawer (min(300px, 84vw)) over a blurred scrim and a menu button appears in the top bar; at 760px tiles stack, gutters tighten to 14px, panels pad 16px, and the chart drops to 230px. The auth layout is a two-column intro-plus-card grid (max 1120px) that collapses to the card alone at 860px.

Spacing runs on a 4px base with 8, 10, 12, 14 for intra-component gaps and 20 to 22 for panel rhythm.

## Elevation & Depth

Depth is glass over glow: fixed radial gradients light the ground, and surfaces sit above it through translucency, backdrop blur (10 to 20px), a pale 1px border, and one long soft drop shadow that pools beneath. Shadows are ambient and dark-indigo tinted, never black and never offset hard. Focus is expressed with a violet halo, not elevation.

### Shadow Vocabulary
- **Panel pool** (`box-shadow: 0 18px 40px -18px rgba(3, 2, 20, .75), 0 2px 6px rgba(3, 2, 20, .35)`): panels, tiles, the nav panel.
- **Readout float** (`box-shadow: 0 18px 36px -12px rgba(3, 2, 20, .8)`): the cash-flow hover readout.
- **Card lift** (`box-shadow: 0 30px 60px -24px rgba(3, 2, 20, .9)`): the auth card, the only surface standing alone on the ground.
- **Primary glow** (`box-shadow: 0 8px 20px -8px rgba(109, 92, 242, .9)`): primary buttons; the active nav item carries a softer violet glow (`0 6px 18px rgba(64, 40, 190, .35)`).
- **Focus halo** (`box-shadow: 0 0 0 3px rgba(139, 123, 255, .25)`; .35 on buttons): inputs, checkboxes, buttons.

### Named Rules
**The Frosted Edge Rule.** Every floating surface gets translucency, a blur, and a 1px Haze Border together. A glass fill without the border, or a border on an opaque fill, breaks the world.

## Shapes

Soft, generous rounding scaled to object size: 6px for bars and inline code, 10px for small buttons, icon wells, and file buttons, 12px for buttons, inputs, nav items, and transaction rows, 14px for alerts, attention rows, search, the alerts chip, and the chart readout, 22px for panels, tiles, the nav panel, and the auth card. Pills (999px) are for tags and count values; avatars and chart dots are circles. Swatches are 10px squares at 3px radius. The logo mark is a 34px gradient tile at 10px radius.

Icons are line drawings on a 24px grid, 1.75 stroke, round caps and joins, inheriting currentColor; they appear at 15 to 18px inline and 32px in empty states.

## Components

### Buttons
Confident, filled, slightly glowing.
- **Shape:** gently rounded (12px; 10px for small).
- **Primary:** Ultraviolet Signal fill, white text, translucent lavender border, primary glow, 700 weight, 0.5rem 1rem padding (48px tall in the auth card).
- **Hover / Focus:** fill brightens to #7c6cff, border lightens; focus adds the violet halo; active nudges down 1px; disabled at 50% opacity.
- **Outline:** near-transparent glass fill with Haze Border Strong; hover raises the fill to 10% white.
- **Danger:** coral fill with dark wine text; outline-danger is a coral-tinted glass version.
- **Link:** Moonlit Link, no underline, 600 weight.

### Chips (tags and counts)
- **Tag:** pill, 0.72rem 700, Mist text on 7% white glass with a Haze Border.
- **Count badge:** an 18px coral pill with dark text, ringed in Solid Indigo, pinned to the bell icon in the alerts chip.

### Cards / Containers
- **Corner Style:** 22px.
- **Background:** Glass with a 16px backdrop blur.
- **Shadow Strategy:** panel pool (see Elevation).
- **Border:** 1px Haze Border.
- **Internal Padding:** 20px 22px (16px on small screens); panel heading row sits 14px above content with an optional right-side link or legend.

### KPI Tile (signature)
Pastel gradient tile with a 50% white border, label, figure, a month-over-month delta with an up/down/flat icon, and an absolutely positioned 62px area spark filling the bottom edge (line in the tile's `-line` colour, fill fading from 55% to 8% opacity). The delta is coloured by meaning: more income is good, more spending is bad.

### Cash-flow Horizon (signature)
Two smoothed SVG lines (2.5 stroke) in the series colours over faint grid lines, HTML axis ticks in Dim Mist. Each month is a focusable column: hover or focus reveals a gradient guide line, white-ringed dots that scale in, and a glass readout (blur 14px, saturate 1.3) listing income, spending, and net, which un-blurs and rises 6px into place with an expo-out ease (cubic-bezier(.16, 1, .3, 1)). The readout flips to the left near the right edge. At rest the current month's dots stay visible.

### Attention Rows
Glass link rows (14px radius, 12px 14px padding) with a 34px tinted icon well, title plus two-line-clamped meta, a pill count, and a chevron. Each row takes one tone (coral, amber, violet, mint) applied to the icon well and count only.

### Lists, Tables, and Bars
- **Transaction rows:** 4% white fill, transparent border that appears on hover, 12px radius; name truncates, money right-aligned and tabular.
- **Tables:** transparent, Haze Border row rules, 0.7rem 0.6rem cells, uppercase label headers in Dim Mist, a 4.5% white hover.
- **Budget bars:** 8px track at 8% white, fill a violet gradient, shifting to amber at warning and coral when over.
- **Donut:** 132px ring, 14px stroke on an 8% white track, segments thicken to 17px on hover, centre figure and caption overlaid.

### Inputs / Fields
- **Style:** 6% white glass, 1px Haze Border, 12px radius (10px small), Dim Mist placeholder; selects use a Mist chevron and Solid Indigo option lists.
- **Focus:** fill to 9%, border to lavender at 65%, violet halo.
- **Error / Disabled:** invalid fields get a 1px coral outline and pink validation text; disabled drops to 2.5% fill and Dim Mist text.
- **Search:** a 46px glass field with a leading icon; the border lightens on focus-within.

### Navigation
A full-height glass nav panel with a darker violet vertical gradient and 18px blur. Items are 0.94rem 600 Mist text with an 18px icon, 11px 12px padding, 12px radius; hover adds 6% white; active becomes white text on a left-to-right violet gradient with a lavender border and a soft violet glow. The foot holds a user card (blue-violet gradient, peach-to-pink circular initials avatar) and a log-out item. Under 960px it slides in from the left over a scrim.

### Alerts
14px radius, 1px border, 10px blur, tinted 14 to 16% fills in info violet-blue, mint, amber, or coral; used for inline toasts with a leading icon.

## Do's and Don'ts

### Do:
- **Do** put every new surface on glass: Glass fill, 1px Haze Border, backdrop blur, and the panel pool shadow, at 22px radius for containers.
- **Do** keep income mint and spending apricot everywhere a series appears, and reserve coral for flags, overspend, and destructive actions.
- **Do** set every currency value with tabular numerals and no wrapping; positive in #6fe3b8, negative in #ff9ca6 on dark.
- **Do** draw new icons on the 24px grid at 1.75 stroke with round caps, inheriting currentColor.
- **Do** open pages with the page head (title, Mist subtitle, actions right) and compose dashboards on the 12-column, 20px-gap grid.
- **Do** reveal hover detail on focus as well, and keep the reduced-motion override intact.

### Don't:
- **Don't** add light-ground cards inside the shell; the three KPI tiles are the only pastel surfaces.
- **Don't** use mint, coral, or amber decoratively, or let a status colour fill a large area.
- **Don't** introduce a second typeface or set money in proportional figures.
- **Don't** place uppercase tracked labels above headings; caps live only in table column headers.
- **Don't** use black or hard-offset shadows; depth is soft, indigo-tinted, and pooled below.
- **Don't** pull in a chart library or raster chart images; charts are SVG plus HTML in this system's colours.
