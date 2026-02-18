// Initialiser Stripe avec la clé publique passée depuis le Razor
const stripe = Stripe(stripePublicKey);
const elements = stripe.elements();
const cardElement = elements.create('card');

// Monter le Card Element
cardElement.mount('#card-element');

// Afficher les erreurs en temps réel
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
if (form) {
    form.addEventListener('submit', handleSubmit);
}

async function handleSubmit(e) {
    e.preventDefault();

    // Récupérer les valeurs du formulaire
    const cardholderName = document.getElementById('cardHolder').value;
    const email = document.getElementById('email').value;
    const postalCode = document.getElementById('postalCode').value;

    // Récupérer les éléments UI
    const submitButton = document.getElementById('submit-btn');
    const errorDiv = document.getElementById('card-errors');

    // Désactiver le bouton pendant la requête
    submitButton.disabled = true;
    errorDiv.textContent = '';

    try {
        // Étape 1: Créer le PaymentIntent (appel au backend)
        const response = await fetch(window.location.href, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                cardholderName: cardholderName,
                email: email
            })
        });

        if (!response.ok) {
            throw new Error(`Erreur serveur: ${response.status}`);
        }

        const data = await response.json();

        if (!data.clientSecret) {
            throw new Error('Pas de clientSecret reçu du serveur');
        }

        const clientSecret = data.clientSecret;

        // Étape 2: Confirmer le paiement avec Stripe
        const result = await stripe.confirmCardPayment(clientSecret, {
            payment_method: {
                card: cardElement,
                billing_details: {
                    name: cardholderName,
                    email: email,
                    address: {
                        postal_code: postalCode
                    }
                }
            }
        });

        if (result.error) {
            // Afficher l'erreur Stripe
            errorDiv.textContent = result.error.message;
            submitButton.disabled = false;
        } else if (result.paymentIntent.status === 'succeeded') {
            // Paiement réussi! Créer la commande
            const orderResponse = await fetch(window.location.href + '?handler=ConfirmOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    paymentIntentId: result.paymentIntent.id
                })
            });

            const orderData = await orderResponse.json();

            if (orderResponse.ok && orderData.orderId) {
                // Rediriger vers la page de commande
                window.location.href = `/Orders/Details?id=${orderData.orderId}`;
            } else {
                throw new Error(orderData.error || 'Erreur création commande');
            }
        } else {
            throw new Error('Le paiement n\'a pas été confirmé');
        }
    } catch (error) {
        errorDiv.textContent = `Erreur: ${error.message}`;
        submitButton.disabled = false;
    }
}
