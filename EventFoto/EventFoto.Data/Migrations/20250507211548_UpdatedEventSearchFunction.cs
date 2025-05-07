using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEventSearchFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION public.event_list_search(
                    offset_id INTEGER,
                    offset_date TIMESTAMPTZ,
                    name_filter TEXT DEFAULT NULL,
                    from_date TIMESTAMPTZ DEFAULT NULL,
                    to_date TIMESTAMPTZ DEFAULT NULL,
                    show_archived BOOLEAN DEFAULT NULL,
                    page_size INTEGER DEFAULT 50
                )
                RETURNS TABLE (
                    ""Id"" INTEGER,
                    ""Name"" VARCHAR(255),
                    ""IsArchived"" BOOLEAN,
                    ""StartDate"" TIMESTAMPTZ,
                    ""EndDate"" TIMESTAMPTZ,
                    ""Filename"" TEXT,
                    ""PhotoCount"" INTEGER
                )
                AS $$
                SELECT
                    e.""Id"",
                    e.""Name"",
                    e.""IsArchived"",
                    e.""StartDate"",
                    e.""EndDate"",
                    ep.""Filename"",
                    ep.""PhotoCount""
                FROM public.""Events"" e
                LEFT JOIN (
                    SELECT g.""EventId"", MIN(p.""ProcessedFilename"") AS ""Filename"", COUNT(p.""Id"") As ""PhotoCount""
                    FROM public.""Gallery"" g
                    LEFT JOIN public.""EventPhotos"" p ON p.""GalleryId"" = g.""Id""
                    WHERE p.""ProcessedFilename"" IS NOT NULL
                    GROUP BY g.""EventId""
                ) ep ON ep.""EventId"" = e.""Id""
                WHERE (e.""StartDate"" > offset_date
                  OR (
  	                e.""StartDate"" = offset_date
  	                AND e.""Id"" > offset_id
                  ))
                  AND (
                      name_filter IS NULL
                      OR e.""Name"" ILIKE '%' || name_filter || '%'
                  )
                  AND (
                      from_date IS NULL
                      OR (
                          e.""StartDate"" >= from_date
                          OR (e.""EndDate"" IS NOT NULL AND e.""EndDate"" >= from_date)
                      )
                  )
                  AND (
                      to_date IS NULL
                      OR (
                          e.""StartDate"" <= to_date
                          OR (e.""EndDate"" IS NOT NULL AND e.""EndDate"" <= to_date)
                      )
                  )
                  AND (
                      show_archived IS TRUE
                      OR (show_archived IS FALSE AND (e.""IsArchived"" = FALSE AND e.""ArchiveName"" IS NULL))
                      OR show_archived IS NULL
                  )
                ORDER BY e.""StartDate"", e.""Id""
                LIMIT page_size;
                $$ LANGUAGE sql STABLE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.event_list_search;");
        }
    }
}
