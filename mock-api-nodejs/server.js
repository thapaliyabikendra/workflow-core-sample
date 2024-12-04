const express = require('express');
const axios = require('axios');
const bodyParser = require('body-parser');
const uuid = require('uuid');  // To generate unique UiPathJobIds
const https = require('https')

const app = express();
const port = process.env.PORT || 3000;
// Middleware to parse JSON bodies
app.use(bodyParser.json());

// In-memory "database" to store UiPath jobs and their statuses
let jobs = {};
const axiosInstance = axios.create({
    https: {
        checkServerIdentity: (host, cert) => {
            // Customize certificate verification here, returning an error if the
            // certificate is invalid or should not be trusted.
        }
    },
    validateStatus: status => {
        // Only reject responses with status codes outside the 2xx range
        return status >= 200 && status < 300;
    },
    httpsAgent: new https.Agent({
        rejectUnauthorized: false
    })
});


// Endpoint to start a UiPath job
app.get('/ui-path/api/start-job', async (req, res) => {
    try {
        // Get taskId from query parameters
        const { taskId } = req.query;
        // Generate a unique UiPath Job ID
        const uiPathJobId = uuid.v4();

        // Initially, the job status is "Pending"
        jobs[uiPathJobId] = { status: 'Pending' };

        console.log(`TaskId = ${taskId}, UiPath Job started: JobId = ${uiPathJobId}, Status = ${jobs[uiPathJobId].status}`);

        // Respond with the UiPath Job ID
        res.status(200).json({
            success: true,
            message: 'UiPath Job started successfully',
            uiPathJobId: uiPathJobId,
            jobStatus: jobs[uiPathJobId].status
        });

        // After 5 seconds, change the status of the job to "Ended"
        // setTimeout(() => {
        //     jobs[uiPathJobId].status = 'Ended';
        //     console.log(`UiPath Job ${uiPathJobId} status updated to "Ended"`);
        // }, 10000);  // Simulate job completion after 5 seconds

    } catch (error) {
        console.error('Error starting UiPath job:', error);
        res.status(500).json({
            success: false,
            message: 'An error occurred while starting the UiPath job',
            error: error.message
        });
    }
});

// Endpoint to update UiPath job status
app.post('/ui-path/api/update-job-status', async (req, res) => {
    try {
        const { uiPathJobId, newStatus } = req.body;

        // Check if the job exists
        if (!jobs[uiPathJobId]) {
            return res.status(404).json({
                success: false,
                message: 'Job not found',
            });
        }

        // Update the status of the job
        jobs[uiPathJobId].status = newStatus;
        console.log(`Job ${uiPathJobId} status updated to ${newStatus}`);

        // Respond with success message
        res.status(200).json({
            success: true,
            message: `Job status updated to ${newStatus}`,
            uiPathJobId: uiPathJobId,
            jobStatus: jobs[uiPathJobId].status,
        });

    } catch (error) {
        console.error('Error updating UiPath job status:', error);
        res.status(500).json({
            success: false,
            message: 'An error occurred while updating the job status',
            error: error.message,
        });
    }
});

// Endpoint to get the status of a UiPath job
app.get('/ui-path/api/job-status', async (req, res) => {
    try {
        // Get UiPathJobId from query parameters
        const { uiPathJobId } = req.query;

        if (!uiPathJobId) {
            return res.status(400).json({
                success: false,
                message: 'uiPathJobId is required'
            });
        }

        // Check if the job exists
        if (!jobs[uiPathJobId]) {
            return res.status(404).json({
                success: false,
                message: `UiPath Job with Id ${uiPathJobId} not found`
            });
        }

        // Get the status of the job
        const jobStatus = jobs[uiPathJobId];

        res.status(200).json({
            success: true,
            uiPathJobId,
            jobStatus: jobStatus.status
        });

    } catch (error) {
        console.error('Error fetching UiPath job status:', error);
        res.status(500).json({
            success: false,
            message: 'An error occurred while fetching the UiPath job status',
            error: error.message
        });
    }
});

// Endpoint to simulate the BPM API call (from your original example)
app.get('/bpm/api/approval-request', async (req, res) => {
    try {
        console.log(`'Request: ${JSON.stringify(req.query)}'`);
        const allowedUserIds = [1, 2];

        if (allowedUserIds.includes(parseInt(req.query.userId))) {
            const successResponse = {
                success: true,
                code: 200,
                message: 'Approval Request received successfully'
            };
            console.log(`'Response: ${JSON.stringify(successResponse)}'`);
            res.status(200).json(successResponse);

            // setTimeout(() => callWebhook(req.query.taskId, 'Approved'), 5000);
        } else {
            const failedResponse = {
                success: false,
                code: 500,
                message: 'Internal Server Error'
            };
            console.log(`'Response: ${JSON.stringify(failedResponse)}'`);
            res.status(500).json(failedResponse);
        }
    } catch (error) {
        console.error('Error calling BPM API:', error);
        const errorResponse = {
            success: false,
            code: error.response ? error.response.status : 500,
            message: error.message || 'An error occurred while calling the BPM API'
        };
        console.log(`'Response: ${JSON.stringify(errorResponse)}'`);
        res.status(errorResponse.code).json(errorResponse);
    }
});

// Utility function to send webhook
async function callWebhook(taskId, approvalStatus) {
    const webhookData = {
        taskId: taskId,
        approvalStatus: approvalStatus
    };

    console.log(`'Request: ${JSON.stringify(webhookData)}'`);

    try {
        const response = await axiosInstance.post(
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
    }
}

// Start the server
app.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});
