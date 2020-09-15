﻿namespace VSS.TRex.Compression
{
    public static class AttributeValueRangeCalculator
    {
        // CalculateAttributeValueRange scans a single attribute across all records in a block of values
        public static void CalculateAttributeValueRange(long[] Values,
                                                        int startIndex, int count,
                                                        long Mask,
                                                        long ANativeNullValue, bool ANullable,
                                                        ref EncodedBitFieldDescriptor FieldDescriptor)
        {
            var ObservedANullValue = false;
            var FirstValue = true;

            FieldDescriptor.NativeNullValue = ANativeNullValue;
            FieldDescriptor.MinValue = ANativeNullValue;
            FieldDescriptor.MaxValue = ANativeNullValue;
            FieldDescriptor.Nullable = ANullable;

            for (int i = startIndex, limit = startIndex + count; i < limit; i++)
            {
                var TestValue = Values[i];

                // Ensure negative values are preserved
                TestValue = TestValue < 0 ? -(-TestValue & Mask) : TestValue & Mask;

                if (FieldDescriptor.Nullable)
                {
                    if (FieldDescriptor.MinValue == ANativeNullValue || (TestValue != ANativeNullValue && TestValue < FieldDescriptor.MinValue))
                        FieldDescriptor.MinValue = TestValue;

                    if (FieldDescriptor.MaxValue == ANativeNullValue || (TestValue != ANativeNullValue && TestValue > FieldDescriptor.MaxValue))
                        FieldDescriptor.MaxValue = TestValue;
                }
                else
                {
                    if (FirstValue || TestValue < FieldDescriptor.MinValue)
                        FieldDescriptor.MinValue = TestValue;

                    if (FirstValue || TestValue > FieldDescriptor.MaxValue)
                        FieldDescriptor.MaxValue = TestValue;
                }

                if (!ObservedANullValue && ANullable && TestValue == ANativeNullValue)
                    ObservedANullValue = true;

                FirstValue = false;
            }

            // If the data stream processed contained no null values, then force the
            // nullable flag to false so we don't encode an extra token for a null value
            // that will never be written.
            if (!ObservedANullValue)
                FieldDescriptor.Nullable = false;

            if (FieldDescriptor.Nullable && FieldDescriptor.MaxValue != FieldDescriptor.NativeNullValue)
            {
                FieldDescriptor.MaxValue++;
                FieldDescriptor.EncodedNullValue = FieldDescriptor.MaxValue;
            }
            else
            {
                FieldDescriptor.EncodedNullValue = 0;
            }

            FieldDescriptor.CalculateRequiredBitFieldSize();
        }
    }
}
