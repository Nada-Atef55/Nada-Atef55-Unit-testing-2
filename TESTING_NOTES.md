1. 15 tests all passed
2. i got 97% line coverage
3. i used 
   ## Moq in :- 
            CheckoutServiceTests.[CheckoutAsync_ValidOrder_SavedAndConfirmed()], [CheckoutAsync_ItemOutOfStock_Throws()], [CheckoutAsync_InvalidOrder_ThrowsAndNotSavedOrConfirmed()]

            ReceiptSnapshotTests.[CheckoutAsync_ExampleC_MatchesSnapshot()]

   ## AutoFixture :-
                 CheckoutTestsWithGeneratedData.[CheckoutAsync_AutoFixtureValidOrder()]

   ## Bogus :- 
            CheckoutTestsWithGeneratedData.[CheckoutAsync_ValidOrderUsingBogus_CorrectCustomerInfo()]
4. i see that it is not a problem because this will encourage many customers to reach the threshold to get the discount 
5. i think the project domain needed Product class to store the Stock so we can validate the quantity of each OrderItem



                                    