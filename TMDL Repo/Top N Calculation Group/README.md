# Top N Calculation Group (TMDL)

A generic Top N calculation group for Power BI, based on a pattern **Martin Bubenheimer** showed in his session: a calculation item "Top 5 Kunden nach Umsatz" that ranks the members of the current selection by `SELECTEDMEASURE()` and keeps only the top 5. All credit for the idea goes to Martin.

## Files

- `Top N Calculation Group.tmdl`: paste-ready script for TMDL view (Power BI Desktop or TMDL view on the web).
- `Top N Calculation Group.pbix`: small test model with synthetic data and known answers (ties at position 5, a member without sales), an N slicer and two matrices.

## What the script adds

- N comes from a numeric range parameter `'Top N Count'` (1 to 50, default 5).
- The ranked column is configured once, in the DAX user-defined function `Ranking.Apply` at the top of the script. The calculation group ranks whichever listed column is on the visual.
- Five calculation items: Top N, Bottom N (skips members without values), Others, Top N Share and Rank (ties counted like TOPN).

## Before you apply it

1. Compatibility level 1702 or higher (DAX user-defined functions).
2. Model property "Discourage implicit measures" = On, otherwise the calculation group is rejected.
3. Replace `'Geography'[ZipCountry]` in `Ranking.Apply` with your own column(s), most detailed first. The last line is the default column for cards and grand totals.
4. Change precedence 20 if another calculation group already uses it.

## Known limits

- A column that is not listed in `Ranking.Apply` is not ranked.
- Grand totals and cards always use the default column.
- In a hierarchy, Top N Share is calculated within each parent row.

## Tested

In Power BI Desktop (October 2026) against the synthetic model: Top 5 = 460 including the tie, Bottom 5 = 230 without the empty member, Others = 40, Share = 92 %, Rank 5, 5, 7 for the tie, and dynamic dispatch to a second column.

Two lessons from that test: a function namespace must not start like a built-in function (`TopN.Apply` is read as `TOPN` and can never be called), and a parameter must not be named `value` (`VALUE` is a DAX function).
