using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchVectorTriggerAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Extensions ──────────────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            // ── search_vector: drop plain column, re-add as GENERATED ALWAYS STORED ─
            // InitialSchema created it as a plain tsvector. We drop and re-add it as
            // a computed column so the DB maintains it automatically on every INSERT/UPDATE.
            migrationBuilder.Sql("ALTER TABLE products DROP COLUMN IF EXISTS search_vector;");
            migrationBuilder.Sql(@"
                ALTER TABLE products
                ADD COLUMN search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('english', coalesce(name, '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(brand, '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(description, '')), 'C')
                ) STORED;
            ");

            // ── FTS GIN index on search_vector ───────────────────────────────────
            // Must NOT use CONCURRENTLY inside a transaction (EF migrations are transactional).
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_products_search_vector ON products USING GIN(search_vector);");

            // ── Trigram indexes ──────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_products_name_trgm  ON products USING GIN(name  gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_products_brand_trgm ON products USING GIN(brand gin_trgm_ops);");

            // ── JSONB indexes ────────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_products_attributes  ON products         USING GIN(attributes jsonb_path_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_variants_attributes  ON product_variants USING GIN(attributes jsonb_path_ops);");

            // ── Price index ──────────────────────────────────────────────────────
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_products_base_price ON products(base_price) WHERE deleted_at IS NULL;");

            // ── Seed: 10 Categories ──────────────────────────────────────────────
            migrationBuilder.Sql(@"
                INSERT INTO categories (id, name, slug, description, sort_order, created_at, updated_at)
                VALUES
                    ('11111111-0001-0000-0000-000000000000', 'Men',         'men',           'Men''s clothing and accessories',   1,  NOW(), NOW()),
                    ('11111111-0002-0000-0000-000000000000', 'Women',       'women',         'Women''s clothing and accessories', 2,  NOW(), NOW()),
                    ('11111111-0003-0000-0000-000000000000', 'Kids',        'kids',          'Children''s clothing',              3,  NOW(), NOW()),
                    ('11111111-0004-0000-0000-000000000000', 'T-Shirts',    'men-t-shirts',  'Men''s T-Shirts',                   1,  NOW(), NOW()),
                    ('11111111-0005-0000-0000-000000000000', 'Jeans',       'men-jeans',     'Men''s Jeans & Denim',              2,  NOW(), NOW()),
                    ('11111111-0006-0000-0000-000000000000', 'Dresses',     'women-dresses', 'Women''s Dresses',                  1,  NOW(), NOW()),
                    ('11111111-0007-0000-0000-000000000000', 'Tops',        'women-tops',    'Women''s Tops & Blouses',           2,  NOW(), NOW()),
                    ('11111111-0008-0000-0000-000000000000', 'Sneakers',    'sneakers',      'Sneakers & Athletic Shoes',         4,  NOW(), NOW()),
                    ('11111111-0009-0000-0000-000000000000', 'Accessories', 'accessories',   'Bags, Belts, Hats & More',          5,  NOW(), NOW()),
                    ('11111111-0010-0000-0000-000000000000', 'Sale',        'sale',          'Discounted Items',                  10, NOW(), NOW())
                ON CONFLICT DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                UPDATE categories SET parent_id = '11111111-0001-0000-0000-000000000000'
                WHERE id IN ('11111111-0004-0000-0000-000000000000','11111111-0005-0000-0000-000000000000');
                UPDATE categories SET parent_id = '11111111-0002-0000-0000-000000000000'
                WHERE id IN ('11111111-0006-0000-0000-000000000000','11111111-0007-0000-0000-000000000000');
            ");

            // ── Seed: 20 Products ────────────────────────────────────────────────
            migrationBuilder.Sql(@"
                INSERT INTO products (id, name, slug, description, brand, category_id, base_price, currency, status, attributes, created_at, updated_at)
                VALUES
                    ('22222222-0001-0000-0000-000000000000', 'Classic White Tee',      'classic-white-tee',      'A timeless white cotton t-shirt.',          'BaseCo',    '11111111-0004-0000-0000-000000000000', 29.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0002-0000-0000-000000000000', 'Slim Fit Blue Jeans',     'slim-fit-blue-jeans',     'Slim fit denim jeans in classic blue.',      'DenimCo',   '11111111-0005-0000-0000-000000000000', 79.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0003-0000-0000-000000000000', 'Floral Wrap Dress',       'floral-wrap-dress',       'Light floral print wrap dress.',             'FloraFash', '11111111-0006-0000-0000-000000000000', 59.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0004-0000-0000-000000000000', 'Striped Polo Shirt',      'striped-polo-shirt',      'Classic striped polo shirt.',                'PoloCraft', '11111111-0004-0000-0000-000000000000', 44.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0005-0000-0000-000000000000', 'High Waist Skinny Jeans', 'high-waist-skinny-jeans', 'High waist skinny jeans for women.',         'DenimCo',   '11111111-0005-0000-0000-000000000000', 89.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0006-0000-0000-000000000000', 'Casual Linen Shirt',      'casual-linen-shirt',      'Breathable linen shirt for summer.',         'LinenHouse','11111111-0004-0000-0000-000000000000', 54.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0007-0000-0000-000000000000', 'Maxi Boho Dress',         'maxi-boho-dress',         'Long flowy bohemian dress.',                 'BohoStyle', '11111111-0006-0000-0000-000000000000', 74.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0008-0000-0000-000000000000', 'Graphic Band Tee',        'graphic-band-tee',        'Vintage-style graphic band t-shirt.',        'UrbanRoots','11111111-0004-0000-0000-000000000000', 34.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0009-0000-0000-000000000000', 'Running Sneakers',        'running-sneakers',        'Lightweight performance running sneakers.',  'SpeedStep', '11111111-0008-0000-0000-000000000000', 119.99, 'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0010-0000-0000-000000000000', 'Canvas Tote Bag',         'canvas-tote-bag',         'Durable canvas tote bag.',                   'CarryOn',   '11111111-0009-0000-0000-000000000000', 24.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0011-0000-0000-000000000000', 'Zip-Up Hoodie',           'zip-up-hoodie',           'Warm zip-up hoodie with kangaroo pocket.',   'CozyWear',  '11111111-0001-0000-0000-000000000000', 64.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0012-0000-0000-000000000000', 'Flare Leg Trousers',      'flare-leg-trousers',      'Wide-leg flare trousers.',                   'FloraFash', '11111111-0002-0000-0000-000000000000', 69.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0013-0000-0000-000000000000', 'Leather Belt',            'leather-belt',            'Full-grain leather belt.',                   'BeltMaster','11111111-0009-0000-0000-000000000000', 39.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0014-0000-0000-000000000000', 'Kids Rainbow T-Shirt',    'kids-rainbow-t-shirt',    'Colorful rainbow t-shirt for kids.',         'TinyTrends','11111111-0003-0000-0000-000000000000', 19.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0015-0000-0000-000000000000', 'Denim Jacket',            'denim-jacket',            'Classic denim jacket with worn finish.',      'DenimCo',   '11111111-0001-0000-0000-000000000000', 99.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0016-0000-0000-000000000000', 'Summer Shorts',           'summer-shorts',           'Light cotton summer shorts.',                'BaseCo',    '11111111-0001-0000-0000-000000000000', 34.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0017-0000-0000-000000000000', 'Silk Blouse',             'silk-blouse',             'Elegant silk blouse.',                       'LuxFash',   '11111111-0007-0000-0000-000000000000', 84.99,  'USD', 'Draft',   '{}', NOW(), NOW()),
                    ('22222222-0018-0000-0000-000000000000', 'Cargo Pants',             'cargo-pants',             'Utility cargo pants with multiple pockets.', 'UrbanRoots','11111111-0001-0000-0000-000000000000', 74.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0019-0000-0000-000000000000', 'Knit Beanie',             'knit-beanie',             'Warm wool blend knit beanie.',                'CozyWear',  '11111111-0009-0000-0000-000000000000', 22.99,  'USD', 'Active',  '{}', NOW(), NOW()),
                    ('22222222-0020-0000-0000-000000000000', 'Sale Yoga Pants',         'sale-yoga-pants',         'Stretchy yoga pants at a discounted price.', 'FlexFit',   '11111111-0010-0000-0000-000000000000', 29.99,  'USD', 'Active',  '{}', NOW(), NOW())
                ON CONFLICT DO NOTHING;
            ");

            // ── Seed: Variants for first 5 products ──────────────────────────────
            migrationBuilder.Sql(@"
                INSERT INTO product_variants (id, product_id, sku, size, color, color_hex, stock_qty, is_active, created_at, updated_at)
                VALUES
                    ('33333333-0001-0000-0000-000000000000', '22222222-0001-0000-0000-000000000000', 'CWT-S-WHT',  'S',  'White', '#FFFFFF', 50, true, NOW(), NOW()),
                    ('33333333-0002-0000-0000-000000000000', '22222222-0001-0000-0000-000000000000', 'CWT-M-WHT',  'M',  'White', '#FFFFFF', 75, true, NOW(), NOW()),
                    ('33333333-0003-0000-0000-000000000000', '22222222-0001-0000-0000-000000000000', 'CWT-L-WHT',  'L',  'White', '#FFFFFF', 60, true, NOW(), NOW()),
                    ('33333333-0004-0000-0000-000000000000', '22222222-0002-0000-0000-000000000000', 'SFJ-30-BLU', 'M',  'Blue',  '#1E40AF', 40, true, NOW(), NOW()),
                    ('33333333-0005-0000-0000-000000000000', '22222222-0002-0000-0000-000000000000', 'SFJ-32-BLU', 'L',  'Blue',  '#1E40AF', 35, true, NOW(), NOW()),
                    ('33333333-0006-0000-0000-000000000000', '22222222-0003-0000-0000-000000000000', 'FWD-S-FLO',  'S',  'Floral','#F472B6', 30, true, NOW(), NOW()),
                    ('33333333-0007-0000-0000-000000000000', '22222222-0003-0000-0000-000000000000', 'FWD-M-FLO',  'M',  'Floral','#F472B6', 25, true, NOW(), NOW()),
                    ('33333333-0008-0000-0000-000000000000', '22222222-0004-0000-0000-000000000000', 'SPS-M-STR',  'M',  'Navy',  '#1E3A5F', 45, true, NOW(), NOW()),
                    ('33333333-0009-0000-0000-000000000000', '22222222-0004-0000-0000-000000000000', 'SPS-L-STR',  'L',  'Navy',  '#1E3A5F', 40, true, NOW(), NOW()),
                    ('33333333-0010-0000-0000-000000000000', '22222222-0005-0000-0000-000000000000', 'HWJ-XS-BLK', 'XS', 'Black', '#000000', 20, true, NOW(), NOW())
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM product_variants WHERE id::text LIKE '33333333-%';");
            migrationBuilder.Sql("DELETE FROM products WHERE id::text LIKE '22222222-%';");
            migrationBuilder.Sql("DELETE FROM categories WHERE id::text LIKE '11111111-%';");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_products_search_vector;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_products_name_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_products_brand_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_products_attributes;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_variants_attributes;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_products_base_price;");
            migrationBuilder.Sql("ALTER TABLE products DROP COLUMN IF EXISTS search_vector;");
            migrationBuilder.Sql("ALTER TABLE products ADD COLUMN search_vector tsvector;");
        }
    }
}
