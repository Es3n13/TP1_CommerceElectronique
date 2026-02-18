namespace BoutiqueElegance.wwwroot.js
{
    public class stripe_payment
    {
        // Initialiser Stripe
        const stripe = Stripe('pk_test_51Sz4rwD8j80AoLJiDLsC5s0fC8CVmKNnaKn5FnpHuEuMH1C4Ah5JZGL8mPuOdhm3MdnNnMGQ8LpEQMLeYgOzZ5q000dtUaDAEJ');
        const elements = stripe.elements();
        const cardElement = elements.create('card');
        cardElement.mount('#card-element');

// Afficher les erreurs du formulaire
    cardElement.on('change', (event) => {
        const displayError = document.getElementById('card-errors');
        if (event.error) {
            displayError.textContent = event.error.message;
        } else {
            displayError.textContent = '';
        }
    });

    // Soumettre le formulaire
    const form = document.getElementById('payment-form');
    form.addEventListener('submit', handleSubmit);

    async function handleSubmit(e) {
        e.preventDefault();

        // Récupérer les valeurs du formulaire
        const cardholderName = document.getElementById('cardHolder').value;
        const email = document.getElementById('email').value;

        // Désactiver le bouton pendant la requête
        const submitButton = document.getElementById('submit-btn');
        submitButton.disabled = true;

        try {
            // Étape 1: Créer le PaymentIntent
            const response = await fetch('/Checkout?handler=CreatePaymentIntent', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    cardholderName: cardholderName,
                    email: email
                })
            });

            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.error || 'Erreur serveur');
            }

            const clientSecret = data.clientSecret;

            // Étape 2: Confirmer le paiement avec Stripe
            const result = await stripe.confirmCardPayment(clientSecret, {
                payment_method: {
                    card: cardElement,
                    billing_details: {
                        name: cardholderName,
                        email: email
                    }
                }
            });

            if (result.error) {
                // Afficher l'erreur Stripe
                document.getElementById('card-errors').textContent = result.error.message;
                submitButton.disabled = false;
            } else if (result.paymentIntent.status === 'succeeded') {
                // Si paiement réussi Créer la commande
                const orderResponse = await fetch('/Checkout?handler=ConfirmOrder', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        paymentIntentId: result.paymentIntent.id
                    })
                });

                const orderData = await orderResponse.json();
                if (orderResponse.ok) {
                    // Rediriger vers la page de commande
                    window.location.href = `/Orders/Details?id=${orderData.orderId}`;
                } else {
                    throw new Error(orderData.error || 'Erreur création commande');
                }
            }
        } catch (error) {
            document.getElementById('card-errors').textContent = error.message;
            submitButton.disabled = false;
        }
    }

    }
}
