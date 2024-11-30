const express = require('express');
const axios = require('axios');
const bodyParser = require('body-parser');

const app = express();
const port = process.env.PORT || 3000;

// Middleware to parse JSON bodies
app.use(bodyParser.json());

// Endpoint to simulate the BPM API call
app.post('/bpm/api/approval-request', async (req, res) => {
    try {
        // Define your BPM API request configuration (this can also be dynamic)
        console.log(`'Request: ${JSON.stringify(req.query)}'`);

        if (req.query.userId == 1) {
            const successResponse = {
                success: true,
                code: 200,
                message: 'Approval Request received successfully'
            };
            console.log(`'Response: ${JSON.stringify(successResponse)}'`);
            res.status(200).json(successResponse);

            setTimeout( () => callWebhook(taskId, 'Approved'), 5000); 
        } else {
            const failedResponse = {
                success: false,
                code: 500,
                message: 'Internal Server Error'
            };
            console.log(`'Response: ${JSON.stringify(failedResponse)}'`);
            res.status(200).json(failedResponse);
        }
    } catch (error) {
        console.error('Error calling BPM API:', error);
        // Send a failure response in case of an error
        const errorResponse = {
            success: false,
            code: error.response ? error.response.status : 500,
            message: error.message || 'An error occurred while calling the BPM API'
        };
        console.log(`'Response: ${JSON.stringify(errorResponse)}'`);
        res.status(errorResponse.Code).json(errorResponse);
    }
});

// Start the server
app.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});

async function callWebhook(taskId, approvalStatus) {
    const webhookData = {
        TaskId: taskId,
        ApprovalStatus: approvalStatus
    };

    try {
        // Send the webhook request to the specified endpoint
        const response = await axios.post(
            'http://localhost:5283/api/webhook',
            webhookData,
            {
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            }
        );

        console.log('Webhook response:', response.data);
        return response.data;
    } catch (error) {
        console.error('Error sending webhook:', error);
        throw error; // Rethrow error to handle it elsewhere if needed
    }
}